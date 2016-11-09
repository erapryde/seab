using Autofac;
using Bot.Dialogs;
using Bot.Utilities;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Bot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
#if ENABLE_DEPENDENCY_INJECTION
        public MessagesController(ILifetimeScope scope)
        {
            SetField.NotNull(out this.scope, nameof(scope), scope);
        }

        public async Task<HttpResponseMessage> Post([FromBody] Activity activity, CancellationToken token)
        {
            if (activity != null & activity.Type == ActivityTypes.Message)    
            {
                //Collect telemtry data 
                WebApiApplication.Telemetry.TrackTrace("UserMessageReceived",
                    Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose,
                    new Dictionary<string, string> { { "Message", activity.Text } });
                try {
                    using (var scope = DialogModule.BeginLifetimeScope(this.scope, activity))
                    {
                        try
                        {
                            var postToBot = scope.Resolve<IPostToBot>();
                            await postToBot.PostAsync(activity, token);
                            //await Conversation.SendAsync(activity, () => new LicOneDialog());
                        }
                        catch (Exception e)
                        {
                            WebApiApplication.Telemetry.TrackTrace("Exception encountered",
                                Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose,
                                new Dictionary<string, string> { { "MesssageController", e.Message } });
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Damn:");
                }
            }
            else
            {
                await HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
#else
        // TODO: "service locator"
        //private readonly ILifetimeScope scope;
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
              
                await Conversation.SendAsync(activity, () => new RecLicDialog());
            }
            else
            {
                await HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            WebApiApplication.Telemetry.TrackEvent("SystemMessagReceived",
                new Dictionary<string, string> { { @"Type", message.Type } });

            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
                if (message.Action.Equals("add", StringComparison.OrdinalIgnoreCase))
                {
                    await Conversation.SendAsync(message, () => new RecLicDialog());
                }
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
#endif
    }
}