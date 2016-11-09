using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Serialization;

namespace Bot.Utilities
{
    public static class Helper
    {
        public static MessageCollection ReadConfig()
        {
            string path = HostingEnvironment.ApplicationPhysicalPath;
            if (path == null) path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "config", "BotConfiguration.xml");

            if (!File.Exists(path))
            {
                throw new Exception("File does not exists " + path);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(MessageCollection));
            StreamReader reader = new StreamReader(path);
            MessageCollection res = (MessageCollection)serializer.Deserialize(reader);
            reader.Close();
            return res; 
        }
    }

    public static class AttributesHelperExtension
    {
        public static string ToDescription(this Enum value)
        {
            var da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return da.Length > 0 ? da[0].Description : value.ToString();
        }
    }

    public class WCFProxyHelper
    {
        private static Dictionary<Type, object> lookup = new Dictionary<Type, object>();
        private static WCFProxyHelper instance = null;

        public static WCFProxyHelper GetInstance()
        {
            if (instance == null)
            {
                instance = new WCFProxyHelper();
            }
            return instance;
        }

        public static Binding ResolveBinding(string name)
        {
            BindingsSection clientSection = (BindingsSection)WebConfigurationManager.GetSection("system.serviceModel/bindings");

            foreach (var bindingCollection in clientSection.BindingCollections)
            {
                IBindingConfigurationElement bindingElement = null;
                if (bindingCollection.ConfiguredBindings.Count > 0 &&
                    (bindingElement = bindingCollection.ConfiguredBindings.FirstOrDefault(x => x.Name == name)) != null)
                {
                    var binding = (Binding)Activator.CreateInstance(bindingCollection.BindingType);
                    binding.Name = bindingElement.Name;
                    bindingElement.ApplyConfiguration(binding);
                    return binding;
                }
            }
            return null;
        }

        public static Uri ResolveEndpointBehavior(string name)
        {
            ClientSection section = (ClientSection)WebConfigurationManager.GetSection("system.serviceModel/client");
            foreach (ChannelEndpointElement element in section.Endpoints)
            {
                if (element.Name == name)
                {
                    return element.Address;
                }
            }
            return null;
        }

        public T GetChannel<T>()
        {
            Type type = typeof(T);
            ChannelFactory<T> factory;

            if (!lookup.ContainsKey(type))
            {
                //if (type == typeof(LicenceOneAPI.IeAdvisor))
                //{
                //    var binding = ResolveBinding("basicHttpBinding");
                //    factory = new ChannelFactory<T>(binding, ResolveEndpointBehavior("basicHttpBinding_IeAdvisor").ToString());
                //}
                //else if (type == typeof(LicenceOneAPI.ILicenceMgt))
                //{
                //    var binding = ResolveBinding("basicHttpBinding");
                //    factory = new ChannelFactory<T>(binding, ResolveEndpointBehavior("basicHttpBinding_ILicenceMgt").ToString());
                //}
                //else if (type == typeof(LicenceOneAPI.IServiceMgt))
                //{
                //    var binding = ResolveBinding("basicHttpBinding");
                //    factory = new ChannelFactory<T>(binding, ResolveEndpointBehavior("basicHttpBinding_IServiceMgt").ToString());
                //}
                //else throw new NotImplementedException(string.Format("Type not found: {0}", typeof(T).ToString()));
                //lookup.Add(type, factory);
                factory = null;
            }
            else
            {
                factory = (ChannelFactory<T>)lookup[type];
            }

            T proxy = factory.CreateChannel();
            ((IClientChannel)proxy).Open();

            return proxy;
        }

        public void CloseChannel(IClientChannel proxy)
        {
            proxy.Close();
        }
    }
    public static class LuisHelper
    {
        public const double CONFIDENCE_THRESHOLD = 0.75;

        public const string STR_BUSINESSINTENT = "BUSINESS_INTENT";

        public const string STR_SECTOR = "SECTOR";

        //public const string STR_LIC_LISTING = "LICENCE_LISTING";

        //TODO: evaluate if this is required 
        public const string STR_LUIS_DATA = "LUIS_DATA";
        public enum CLARIFICATION_STATE
        {
            EXIT,
            SUCCESS,
            FAILED
        }
        public enum INTENT_TYPE
        {
            [Description("Enquire Application")]
            ENQUIRE_APPLICATION,
            [Description("MainMenu")]
            MAINMENU
        }
        //TODO: to evaluate if this is required still
        //public enum BUSINESS_TYPE
        //{
        //    [Description("BusinessType::F&B")]
        //    FNB,
        //    [Description("BusinessType::HealthCare")]
        //    Healthcare,
        //    [Description("BusinessType::Oil&Gas")]
        //    Petrochemical
        //}

        //singleton pattern
        private static LuisClient client;
        internal static LuisClient GetLuisInstance()
        {
            if (client == null)
            {
                client =
                        new LuisClient(System.Configuration.ConfigurationManager.AppSettings["LuisClientAppID"].ToString(),
                        System.Configuration.ConfigurationManager.AppSettings["LuisClientAppKey"].ToString(),
                        true
                        );
            }
            return client;
        }
    }
}