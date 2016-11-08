//using AuthBot;
//using AuthBot.Dialogs;
//using AuthBot.Models;
using Bot.Dialogs.PayFine;
//using Bot.LicenceMgtAPI;
using Bot.Utilities;
using BusinessObjects.LicenceMgt;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Dialogs
{
    

    [Serializable]
#if ENABLE_DEPENDENCY_INJECTION
    public class LicOneDialog : LuisDialog<Object> 
#else
    public class RecLicDialog : IDialog<Object>
#endif
    {
        #region telemetry constants 

        private const string TELE_HANDLED = "Handled";

        private const string TELE_USER_TEXT = "User Text";

        private const string TELE_SELECTED_CHOICE = "SelectedChoice";

        private const string TELE_USER_CHOICE = "LicOneDialog-UserChoice";

        #endregion



        #region class attributes 

        private bool init = false;

        private int choice = -1;

        private string str_luis_clarification;

        private string str_luis_reply;

        private bool? bclarifyIntentWithLuis;

        private bool bUserChoice_gotomain;

        #endregion 

#if ENABLE_DEPENDENCY_INJECTION
        #region DEPENDENCY_INJECTION CODE SEGMENT

        private readonly IEntityToType entityToType;
        public LicOneDialog(IEntityToType entityToType, ILuisService luis)
            : base(luis)
        {
            SetField.NotNull(out this.entityToType, nameof(entityToType), entityToType);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Name));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("RecommendLicence")]
        public async Task RecommendLicence(IDialogContext context, LuisResult result)
        {
            //No further question required on the intent 
            if(result.DialogResponse.Status == "Finished")
            {
                //evaluate result from Luis 
                var act = result.TopScoringIntent.Actions.FirstOrDefault();
                if(act != null && act.Triggered)
                {
                     //intent triggered, proceed to check licence 
                }
                else
                {
                    //intent invalid... direct to none intent 
                    await None(context, result);
                }
            }
            //Further question required on the intent 
            else if(result.DialogResponse.Status == "Question")
            {
                var qn = result.DialogResponse.Prompt;
                await context.PostAsync(qn);
            }

            //LicenceRecommendation alarm;
            //if (TryFindAlarm(result, out alarm))
            //{
            //    this.alarmByWhat.Remove(alarm.What);
            //    await context.PostAsync($"alarm {alarm} deleted");
            //}
            //else  
            //{
            //    await context.PostAsync("did not find alarm");
            //}
            //context.Wait(MessageReceived);
        }
        #endregion
#else
        #region Tradition web service call approach

        private void ResetParameters()
        {
            //init parameters 
            //bclarifyIntentWithLuis = null;
            //str_luis_clarification = null;
            //luisdata = null;
            bUserChoice_gotomain = false;
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StartAsync(IDialogContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            ResetParameters();

            try
            {
                //1. Provide instructions for user to enter intent 
                await PostMenu(context);
                //2. Receive user intent and clarify further if further clarifications is needed on the intent 
                context.Wait(AfterUserResponded);
                //3. check with LUIS the intent 

                //4. Check with backend the proposed licences  
            }
            catch(Exception e)
            {
                //TODO: Send this to event tracker
            }
            
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task PostMenu(IDialogContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Can't use PromptDialog.Choice here as that *requires* the user choose from the list of choices made available.
            // What we want is to present a buttons for convenience, or let them type in a shorter (similar) response and handle
            // that in AfterChoiceMadeActivity
            // So we create a custom message here to do that.
            var menuMessage = context.MakeMessage();
            Microsoft.Bot.Connector.Place pl = new Microsoft.Bot.Connector.Place()
            {
                Type = "Place",
                Name = "LicenceOne",
                HasMap = "https://goo.gl/maps/AKvT572BND42",
                Geo = new GeoCoordinates(null, 1.27, 103.65),
                Address = "31 Science Park Rd, Singapore 117611"
            };
            Microsoft.Bot.Connector.Entity ent = new Microsoft.Bot.Connector.Entity("Place");
            ent.SetAs<Place>(pl);
            //menuMessage.Entities.Add(ent);
            menuMessage.Attachments = new List<Attachment> { CreateMainMenu().ToAttachment() };
            await context.PostAsync(menuMessage);
        }

        /// <summary>
        /// Handle all responses to the dialog 
        /// </summary>
        /// <param name="context">The context of the conversation</param>
        /// <param name="result">The result obtained from the previous step (Predict/Reply)</param>
        /// <returns>return null if user decided to quit</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task AfterUserResponded(IDialogContext context, IAwaitable<IMessageActivity> result)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var content = (await result).Text;

            //Set up telemetry 
            EventTelemetry choiceEvent = new EventTelemetry(@"LicOneDialog-UserChoice");
            choiceEvent.Properties.Add(@"User Text", content);
            const string SELECTED_CHOICE_PROPERTY_KEY = @"SelectedChoice";
            const string HANDLED_PROPERTY_KEY = @"Handled";
            choiceEvent.Properties.Add(HANDLED_PROPERTY_KEY, bool.TrueString);
            choiceEvent.Properties.Add(SELECTED_CHOICE_PROPERTY_KEY, @"Unknown");

            LuisResult res = null;
            try
            {
                //Check Luis for intent 
                res = await LuisHelper.GetLuisInstance().Predict(content);
            }
            catch(Exception ex)
            {
              Console.WriteLine("Exception " + ex.Message);
            }

            if (res != null)
            {
                Microsoft.Cognitive.LUIS.Action act;
                if ((act = res.TopScoringIntent.Actions.FirstOrDefault()) != null && act.Triggered)
                {
                    //require further info from user 
                    if (res.DialogResponse != null && res.DialogResponse.Status == "Question")
                    {
                        choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"Clarifying Intent";
                        WebApiApplication.Telemetry.TrackEvent(choiceEvent);

                       // this.str_luis_clarification = res.DialogResponse.Prompt;
                        //this.bclarifyIntentWithLuis = true;
                        context.PrivateConversationData.SetValue(LuisHelper.STR_LUIS_DATA, res.DialogResponse);
                        // context.PrivateConversationData.SetValue(STR_)
                        context.Call(new ClarifyIntentDialog(), ClarificationComplete);
                        //while there is user clarification required....
                        //if (this.bclarifyIntentWithLuis.Value)
                        //{
                        //    context.Call(new PayFlow(), ClarificationComplete);
                        //    await AfterUserClarifiedIntent
                        //}
                    }
                    else if (res.DialogResponse != null && res.DialogResponse.Status != "Question")
                    {
                        //Go on to handle usual intents in remaining part of function below
                        //Handle known intents here 
                        //Process only if top scoring intent > 50% accuracy 
                        if (res.TopScoringIntent != null )
                        {
                            //Get intent name, intent score
                            if (res.TopScoringIntent.Name == LuisHelper.INTENT_TYPE.ENQUIRE_APPLICATION.ToDescription())
                            {
                                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = LuisHelper.INTENT_TYPE.ENQUIRE_APPLICATION.ToDescription();
                                WebApiApplication.Telemetry.TrackEvent(choiceEvent);
                                choice = 0;

                                if(res.Entities.Count == 0)
                                {
                                    await context.PostAsync("Please specify the nature of business e.g. setup foodstall business");
                                    context.Wait(AfterUserResponded);
                                }
                                else
                                {
                                    for (int i = 0; i < res.Entities.Count; ++i)
                                    {
                                        string businessType = res.Entities.ElementAt(i).Key;
                                        //Get backend questionaires for business type 
                                        context.PrivateConversationData.SetValue(LuisHelper.STR_SECTOR, res.Entities.ElementAt(i).Key);
                                        if (res.Entities.ElementAt(i).Value.Count() > 0)
                                        {
                                            string val = res.Entities.ElementAt(i).Value.FirstOrDefault().Value;
                                            context.PrivateConversationData.SetValue(LuisHelper.STR_BUSINESSINTENT, val);
                                        }

                                        context.Call(new eAdvisorDialog(), GuidedQuestionComplete);
                                    }
                                }
                            }
                            else if(res.TopScoringIntent.Name == LuisHelper.INTENT_TYPE.ENQUIRE_APPLICATION.ToDescription())
                            {
                                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = LuisHelper.INTENT_TYPE.ENQUIRE_APPLICATION.ToDescription();
                                WebApiApplication.Telemetry.TrackEvent(choiceEvent);
                                choice = 1;

                                await context.PostAsync("Please enter the application #");

                                context.Wait(AfterOpenEndedAnswer);

//                                PromptDialog.Text(context, AfterOpenEndedAnswer, "Please enter the application #", null, 3);
                            }
                        }
                        else//intent not found
                        {
                            choiceEvent.Properties[HANDLED_PROPERTY_KEY] = bool.FalseString;
                            WebApiApplication.Telemetry.TrackEvent(choiceEvent);
                            await context.PostAsync("Sorry, I didn't understand. Try one of the options above again, please.");
                            context.Wait(AfterUserResponded);
                        }
                    }
                    else//res.DialogResponse is null 
                    {
                        //TODO: should not happen, low priority
                        choiceEvent.Properties[HANDLED_PROPERTY_KEY] = bool.FalseString;
                        WebApiApplication.Telemetry.TrackEvent(choiceEvent);

                        await context.PostAsync("Sorry, I didn't understand.. Try one of the options above again, please.");
                        context.Wait(AfterUserResponded);
                    }
                }
                else
                {
                    if (!init)
                    {
                        init = true;
                        context.Wait(AfterUserResponded);
                    }
                    else
                    {
                        await context.PostAsync("Sorry, I didn't understand your intent. Please try again...");
                        context.Wait(AfterUserResponded);
                    }
                }
            }
            else
            {
                //TODO - low priority: Indicates all required info are available for LUIS query
                //Result is null, possibly errors encountered ...
                choiceEvent.Properties[HANDLED_PROPERTY_KEY] = bool.FalseString;
                WebApiApplication.Telemetry.TrackEvent(choiceEvent);
                await context.PostAsync("Sorry, I didn't understand... Try one of the options above again, please.");
                context.Wait(AfterUserResponded);
            }
        }

        private async Task GuidedQuestionComplete(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                BeDone(context);
            }
            else
            {
                await PromptForEntry(context, @"Oops! There appears to have been an error processing your payment.

You can try again or enter more fines to pay now.");
            }
        }

        private async Task ClarificationComplete(IDialogContext context, IAwaitable<object> result)
        {
            LuisHelper.CLARIFICATION_STATE cla_state = (LuisHelper.CLARIFICATION_STATE)(await result);

            //Set up telemetry 
            EventTelemetry choiceEvent = new EventTelemetry(@"LicOneDialog-UserChoice");
            choiceEvent.Properties.Add(@"User Text", "TBD");
            const string SELECTED_CHOICE_PROPERTY_KEY = @"SelectedChoice";
            const string HANDLED_PROPERTY_KEY = @"Handled";
            choiceEvent.Properties.Add(HANDLED_PROPERTY_KEY, bool.TrueString);
            choiceEvent.Properties.Add(SELECTED_CHOICE_PROPERTY_KEY, @"Unknown");

            switch (cla_state)
            {
                case LuisHelper.CLARIFICATION_STATE.EXIT:

                    break;
                case LuisHelper.CLARIFICATION_STATE.FAILED:
                    //suppose to post async or call the clarification flow again 
                    break;
                case LuisHelper.CLARIFICATION_STATE.SUCCESS:
                    {
                        //Dialog res;
                        //context.PrivateConversationData.TryGetValue(LuisHelper.STR_LUIS_DATA, out res);
                        //if (res != null)
                        //{
                        //    await HandleIntent(context, choiceEvent, SELECTED_CHOICE_PROPERTY_KEY, HANDLED_PROPERTY_KEY, res);
                        //    BeDone(context);
                        //}
                        //else
                        //{
                        //    await PromptForEntry(context, @"Oops! There appears to have been an error processing your query. You can try again or enter more fines to pay now.");
                        //}
                    }
                    break;
                default:
                    break;
            }
            
        }

        private void BeDone(IDialogContext context, bool result = true)
        {
            context.ConversationData.RemoveValue(LuisHelper.STR_LUIS_DATA);
            context.Done(result);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task AfterOpenEndedAnswer(IDialogContext context, IAwaitable<IMessageActivity> result)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var content = (await result).Text;

            //LicenceMgtAPI.LicenceMgtClient client = new LicenceMgtAPI.LicenceMgtClient();
            EnquiryResult res = null;
            try
            {
                switch (choice)
                {
                    case 1:
//                        res = client.EnquireApplication(content, "TODO", "TODO");
                        //res = WCFProxyHelper.GetInstance().GetChannel<LicenceOneAPI.ILicenceMgt>().EnquireApplication(content, "TODO", "TODO");
                        string text;
                        if (res != null && !string.IsNullOrEmpty(res.RefID))
                        {
                            text = "The status of your application is: " + res.AppStatus.ToDescription();
                        }
                        else
                        {
                            text = "Your application is not found, please try again!";
                        }
                        await context.PostAsync(text);
                        break;
                    case 2:
                        //res = client.EnquireLicence(content, "TODO", "TODO");
                        //res = WCFProxyHelper.GetInstance().GetChannel<LicenceOneAPI.ILicenceMgt>().EnquireLicence(content, "TODO", "TODO");
                        string text2; 
                        if (res != null && !string.IsNullOrEmpty(res.RefID))
                        {
                            text2 = "The status of your licence is: " + res.LicStatus.ToDescription();
                            
                        }
                        else
                        {
                            text2 = "Your licence number is not found, please try again!";
                        }
                        await context.PostAsync(text2);
                        break;
                    default:
                        break;
                }
                
                //Go back to main menu 
                await PostMenu(context);
                context.Wait(AfterUserResponded);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception encountered:" + ex.Message);
            }

            return;
        }

#region static method calls 

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
        internal static HeroCard CreateMainMenu()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine();
            b.Append("Hi there! I am Tifa!");
            b.AppendLine();
            b.Append("Here are a few things I can help you with:");

            var menu = new HeroCard()
            {
                Images = new List<CardImage>()
                {
                     new CardImage("https://licence1.business.gov.sg/frontier-theme/images/custom/LicenceOne-Logo.jpg")
                },
                Title = "Welcome to LicenceOne!",
                Text = b.ToString(),
                Buttons = new List<CardAction>()
                {
                    //new CardAction(null, title: @"Recommend licence(s) relevant for your business. Please indicate your business intent e.g. *setup foodstall business*", value: @"RecommendLicence"),
                    new CardAction(ActionTypes.PostBack, title: @"Recommend licence(s) for my business.", value: @"Recommend Licence"),
                    new CardAction(ActionTypes.PostBack, title: @"Enquire status of my application", value: @"Enquire Application"),
                    new CardAction(ActionTypes.PostBack, title: @"Enquire status of my licence", value: @"Enquire Licence"),
                }
            };
           
            //CardImage img = 
            // menu.Buttons.Add(new CardAction(ActionTypes.PostBack, title: @"Recommend me Licence(s) by Business Intent", value: @"Recommend me Licence(s) by Business Intent"));
            //menu.Buttons.Add(new CardAction(ActionTypes.PostBack, title: @"Apply for a new license", value: @"Apply for a new license"));
            //menu.Buttons.Add(new CardAction(ActionTypes.PostBack, title: @"Renew a license", value: @"Renew a license"));
            return menu;
        }

#endregion

        #endregion


#endif

#region old code for reference 
        //internal static string BuildWelcome()
        //{
        //    var sb = new StringBuilder();
        //    sb.AppendLine(@"Welcome to LicenceOne! Here are a few things I can help you with:");
        //    sb.AppendLine();
        //    sb.AppendLine("* Recommend a Licence for your business");
        //    //            sb.AppendLine("* Renew a license");

        //    return sb.ToString();
        //}
        //internal static HeroCard CreateMainMenuCard()
        //{
        //    var menu = new HeroCard(title: @"Welcome!",
        //        text: @"What would you like to do?")
        //    {
        //        Buttons = new List<CardAction>()
        //    };
        //    menu.Buttons.Add(new CardAction(ActionTypes.PostBack, title: @"Recommend me a licence", value: @"Pay parking fines"));
        //    menu.Buttons.Add(new CardAction(ActionTypes.PostBack, title: @"Apply for a new license", value: @"Apply for a new license"));
        //    menu.Buttons.Add(new CardAction(ActionTypes.PostBack, title: @"Renew a license", value: @"Renew a license"));

        //    return menu;
        //}

        //private async Task MenuPromptAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        //{
        //    //var content = await result;
        //    //if (string.IsNullOrEmpty(await context.GetAccessToken(AuthSettings.Scopes)))
        //    //{
        //    //    await context.PostAsync(BuildWelcome());
        //    //    await context.Forward(new AzureAuthDialog(AuthSettings.Scopes, "Please click to sign in to SingPass: "), this.ResumeAfterAuth, content, CancellationToken.None);
        //    //}
        //    //else
        //    //{
        //    //    await PostMenu(context);
        //    //}
        //    await PostMenu(context);
        //}

        //public async Task Logout(IDialogContext context)
        //{
        //    //If any data was saved to UserData or other data bag they should be removed here. Examples:
        //    //Logs out from AuthBot
        //    await context.Logout();

        //    context.Wait(this.MenuPromptAsync);
        //}
        //private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        //{
        //    await PostMenu(context);
        //}
        
        //#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        //        private async Task AfterChoiceMadeActivity(IDialogContext context, IAwaitable<IMessageActivity> result)
        //#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        //        {
        //            var content = (await result).Text;

        //            const string SELECTED_CHOICE_PROPERTY_KEY = @"SelectedChoice";
        //            const string HANDLED_PROPERTY_KEY = @"Handled";
        //            EventTelemetry choiceEvent = new EventTelemetry(@"LicOneDialog-UserChoice");
        //            choiceEvent.Properties.Add(@"User Text", content);
        //            choiceEvent.Properties.Add(HANDLED_PROPERTY_KEY, bool.TrueString);
        //            choiceEvent.Properties.Add(SELECTED_CHOICE_PROPERTY_KEY, @"Unknown");

        //            if (content.ToLowerInvariant().Contains("pay"))
        //            {
        //                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"Pay";
        //                WebApiApplication.Telemetry.TrackEvent(choiceEvent);

        //                context.Call(new PayFineDialog(), (ctx, rslt) => PostMenu(ctx));
        //                return;
        //            }
        //            else if (content.ToLowerInvariant().Contains("apply"))
        //            {
        //                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"ApplyForLicense";
        //                WebApiApplication.Telemetry.TrackEvent(choiceEvent);

        //                await context.PostAsync("Applying for a license is not yet implemented. Check back soon!");
        //            }
        //            else if (content.ToLowerInvariant().Contains("renew"))
        //            {
        //                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"RenewLicense";
        //                WebApiApplication.Telemetry.TrackEvent(choiceEvent);

        //                await context.PostAsync("Renewing a license is not yet implemented. Check back soon!");
        //            }
        //            else if (content.ToLowerInvariant().Contains("done")
        //                | content.ToLowerInvariant().Contains("logout")
        //                | content.ToLowerInvariant().Contains("bye"))
        //            {
        //                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"Logout";
        //                WebApiApplication.Telemetry.TrackEvent(choiceEvent);

        //                //await Logout(context);
        //                context.Done("Thank you, goodbye!");
        //                return;
        //            }
        //            else
        //            {
        //                choiceEvent.Properties[@"Handled"] = bool.FalseString;
        //                WebApiApplication.Telemetry.TrackEvent(choiceEvent);

        //                await context.PostAsync("Sorry, I didn't understand. Try one of the options above again, please.");
        //            }

        //            context.Wait(AfterChoiceMadeActivity);
        //        }
#endregion
    }
}