using Microsoft.Bot.Builder.Dialogs;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Cognitive.LUIS;
using System.Text;
using Bot.Utilities;

namespace Bot.Dialogs
{
    [Serializable]
    public class ClarifyIntentDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            Dialog dialogresponse;
            context.PrivateConversationData.TryGetValue(LuisHelper.STR_LUIS_DATA, out dialogresponse);
            await context.PostAsync(dialogresponse.Prompt);
            context.Wait(ClarificationReceived);
        }

       
        public async Task ClarificationReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            string content = (await result).Text;

            //TODO: review if telemetry data collected is correct 
            //Set up telemetry 
            EventTelemetry choiceEvent = new EventTelemetry(@"LicOneDialog-UserChoice");
            choiceEvent.Properties.Add(@"User Text", content);
            const string SELECTED_CHOICE_PROPERTY_KEY = @"SelectedChoice";
            const string HANDLED_PROPERTY_KEY = @"Handled";
            choiceEvent.Properties.Add(HANDLED_PROPERTY_KEY, bool.TrueString);
            choiceEvent.Properties.Add(SELECTED_CHOICE_PROPERTY_KEY, @"Unknown");

            if (content == LuisHelper.INTENT_TYPE.MAINMENU.ToDescription())
            {
                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"Response to clarification - go to main menu";
                WebApiApplication.Telemetry.TrackEvent(choiceEvent);

                //TODO: where to go for main menu 
                //this.bUserChoice_gotomain = true;

                context.Done(LuisHelper.CLARIFICATION_STATE.EXIT);

                return;
            }

            //Reply clarification to luis 
            Dialog dialogresponse;
            context.PrivateConversationData.TryGetValue(LuisHelper.STR_LUIS_DATA, out dialogresponse);
            LuisResult luisdata = new LuisResult();
            luisdata.DialogResponse = dialogresponse;
            LuisResult response = await LuisHelper.GetLuisInstance().Reply(luisdata, content);

            //forward reply to Luis
            //require further info from user 
            if (response.DialogResponse != null && response.DialogResponse.Status.ToUpper() == "FINISHED")
            {
                //TODO: to review for analytics
                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"Response to clarification FINISHED";
                WebApiApplication.Telemetry.TrackEvent(choiceEvent);
                context.Done(LuisHelper.CLARIFICATION_STATE.SUCCESS);
            }
            else //Indicates all required info are available for LUIS query
            {
                //TODO: to review for analytics
                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"Response to clarification continuing - " + content;
                WebApiApplication.Telemetry.TrackEvent(choiceEvent);
                context.Done(LuisHelper.CLARIFICATION_STATE.FAILED);
                //this.bclarifyIntentWithLuis = false;
            }
            context.PrivateConversationData.SetValue(LuisHelper.STR_LUIS_DATA, response.DialogResponse);
        }

        private async Task PromptForEntry(IDialogContext context, StringBuilder msgBuilder) => await PromptForEntry(context, msgBuilder?.ToString());
        private async Task PromptForEntry(IDialogContext context, string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) throw new ArgumentNullException(nameof(msg));

            // Can't use PromptDialog.Choice here as that *requires* the user choose from the list of choices made available.
            // What we want is to present a Pay button all the time, or let them put in another vehicle/notice number and go through
            // the Add Notice flow again
            // So we create a custom message here to do that.c
            var cmsg = context.MakeMessage();
            cmsg.Text = msg;
            cmsg.TextFormat = "plain";

            var card = new HeroCard();
            card.Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, @"Clarify Business Intent", value: @"Clarify Business Intent") };
            cmsg.Attachments = new List<Attachment> { card.ToAttachment() };

            await context.PostAsync(cmsg);
        }

    }
}