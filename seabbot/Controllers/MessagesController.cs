using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using SeabBot.Dialog;
using System.Collections.Generic;
using SeabBot.Dialog;
using System.Configuration;

namespace SeabBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new MainDialog());
                
            }
            else
            {
                if (activity.Type == ActivityTypes.ConversationUpdate)
                {
                    if (activity.MembersAdded.Count > 0)
                    {
                        //var replyMessage = activity.CreateReply("Yo, what's up?");
                        //var connector = new ConnectorClient(new Uri(activity.ServiceUrl), ConfigurationManager.AppSettings["MicrosoftAppId"].ToString(), ConfigurationManager.AppSettings["MicrosoftAppPassword"].ToString());
                        //var replyMessage = incomingMessage.CreateReply("Yo, I heard you.", "en");
                        //await connector.Conversations.ReplyToActivityAsync(replyMessage);

                        //var cmsg = .MakeMessage();

                        //HeroCard c = new HeroCard(null, null, "hi there! I am Staffie!\nHere are a few things I can help you with:\n");
                        //List<CardAction> buts = new List<CardAction>()
                        //{
                        //    new CardAction("postback", "Get exam information"),
                        //    new CardAction("postback", "Enquire nearest exam centre"),
                        //    new CardAction("postback", "Book seat from exam centre"),
                        //    new CardAction("postback", "Enquire staff directory"),
                        //    new CardAction("postback", "Enter /main to exit to main menu " )
                        //};
                        //c.Buttons = buts;
                        //return c.ToAttachment();
                    }
                }
                await HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

#pragma warning disable CS1998
        private async Task<Activity> HandleSystemMessage(Activity message)
#pragma warning restore CS1998
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
    }
}