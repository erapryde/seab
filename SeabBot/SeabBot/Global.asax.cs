using Autofac;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace SeabBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
#if ENABLE_DEPENDENCY_INJECTION
            { 

                // http://docs.autofac.org/en/latest/integration/webapi.html#quick-start
                //var builder = new ContainerBuilder();

                ////Microsoft App ID
                //const string BotId = "973a3cd5-e5a0-4877-b3e3-017ee18c32c8";

                //// register the Bot Builder module
                //builder.RegisterModule(new DialogModule());
                //// register some configuration
                //builder.Register(c => new BotIdResolver(BotId)).AsImplementedInterfaces().SingleInstance();
                //// register the alarm dependencies
                //builder.RegisterModule(new eAdvisorModule());

                //// Get your HttpConfiguration.
                //var config = GlobalConfiguration.Configuration;

                //// Register your Web API controllers.
                //builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

                //// OPTIONAL: Register the Autofac filter provider.
                //builder.RegisterWebApiFilterProvider(config);

                //// Set the dependency resolver to be Autofac.
                //var container = builder.Build();
                //config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

                
            }
#endif
            #region Authbot code 
            {
                //AuthBot.Models.AuthSettings.Mode = ConfigurationManager.AppSettings["ActiveDirectory.Mode"];
                //AuthBot.Models.AuthSettings.EndpointUrl = ConfigurationManager.AppSettings["ActiveDirectory.EndpointUrl"];
                //AuthBot.Models.AuthSettings.Tenant = ConfigurationManager.AppSettings["ActiveDirectory.Tenant"];
                //AuthBot.Models.AuthSettings.RedirectUrl = ConfigurationManager.AppSettings["ActiveDirectory.RedirectUrl"];
                //AuthBot.Models.AuthSettings.ClientId = ConfigurationManager.AppSettings["ActiveDirectory.ClientId"];
                //AuthBot.Models.AuthSettings.ClientSecret = ConfigurationManager.AppSettings["ActiveDirectory.ClientSecret"];
                //AuthBot.Models.AuthSettings.Scopes = ConfigurationManager.AppSettings["ActiveDirectory.Scopes"].Split(',');
            }
            #endregion

            // WebApiConfig stuff
            GlobalConfiguration.Configure(config =>
            {
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            });
        }
        //public static ILifetimeScope FindContainer()
        //{
        //    var config = GlobalConfiguration.Configuration;
        //    var resolver = (AutofacWebApiDependencyResolver)config.DependencyResolver;
        //    return resolver.Container;
        //}
        internal static TelemetryClient Telemetry { get; } = new TelemetryClient();
    }
}
