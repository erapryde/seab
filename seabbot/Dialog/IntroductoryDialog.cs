using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SeabBot.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SeabBot.Dialog
{
    [Serializable]
    public class IntroductoryDialog : IDialog<object>
    {
        private STATE state = STATE.L0_ENQUIRE_L1_INTENT;

//        public async Task StartAsync(IDialogContext context)
//#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
//        {
//            try
//            {
//                var menuMessage = context.MakeMessage();
//                menuMessage.Attachments = new List<Attachment> { CreateMainMenu(context) };
//                await context.PostAsync(menuMessage);
//                context.Wait(AfterUserResponded);
//            }
//            catch (Exception)
//            {
//                //TODO: Send this to event tracker
//            }
//        }

        public async Task AfterUserResponded(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var res = (await result).Text;

            context.Done(true);

        }

        internal static Attachment CreateMainMenu(IDialogContext context)
        {
            var cmsg = context.MakeMessage();

            HeroCard c = new HeroCard(null, null, "hi there! I am Staffie!\nHere are a few things I can help you with:\n");
            List<CardAction> buts = new List<CardAction>()
            {
                new CardAction("postback", "Get exam information"),
                new CardAction("postback", "Enquire nearest exam centre"),
                new CardAction("postback", "Book seat from exam centre"),
                new CardAction("postback", "Enquire staff directory"),
                new CardAction("postback", "Enter /main to exit to main menu " )
            };
            c.Buttons = buts;
            return c.ToAttachment();
        }

        public async Task StartAsync(IDialogContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                await PostMenu(context, L1_MAINOPTIONS.BOOK_EXAM_SLOT, Helper.ReadConfig().Message.FirstOrDefault(x => x.ID == "WelcomeMsg").Text);
                context.Wait(MainMenuResponded);
            }
            catch (Exception)
            {
                //TODO: Send this to event tracker
            }
        }


#pragma warning disable CS1998
        public async Task MainMenuEnded(IDialogContext context, IAwaitable<IMessageActivity> result)
#pragma warning restore CS1998
        {

        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task MainMenuResponded(IDialogContext context, IAwaitable<IMessageActivity> result)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var content = (await result).Text;

            #region  Set up telemetry 
            EventTelemetry choiceEvent = new EventTelemetry(@"LicOneDialog-UserChoice");
            choiceEvent.Properties.Add(@"User Text", content);
            const string SELECTED_CHOICE_PROPERTY_KEY = @"SelectedChoice";
            const string HANDLED_PROPERTY_KEY = @"Handled";
            choiceEvent.Properties.Add(HANDLED_PROPERTY_KEY, bool.TrueString);
            choiceEvent.Properties.Add(SELECTED_CHOICE_PROPERTY_KEY, @"Unknown");
            #endregion

            #region button processing 


            switch (state)
            {
                case STATE.L0_ENQUIRE_L1_INTENT:
                    {
                        if (content.ToUpper() == L1_MAINOPTIONS.GET_EXAM_INFO.ToDescription().ToUpper())
                        {
                            state = STATE.L1_GET_EXAM_INFO;
                            await PostMenu(context, L2_EXAMINFOOPTIONS.GET_SYLLABUS_BY_SUBJECT, Helper.ReadConfig().Message.FirstOrDefault(x => x.ID == "L1_ExamInfoMsg").Text);
                        }
                    }
                    break;
                case STATE.L1_GET_EXAM_INFO:
                    {
                        if (content.ToUpper() == L2_EXAMINFOOPTIONS.GET_SYLLABUS_BY_SUBJECT.ToDescription().ToUpper())
                        {
                            state = STATE.L1_GET_EXAM_INFO;
                            await PostMenu(context, L2_EXAMINFOOPTIONS.GET_SYLLABUS_BY_SUBJECT, Helper.ReadConfig().Message.FirstOrDefault(x => x.ID == "L1_ExamInfoMsg").Text);
                        }
                    }
                    break;
                case STATE.L1_ENQUIRE_NEAREST_CENTER:
                    break;
                default:
                    break;
            }
            #endregion

            #region sample code

            //            if (res != null)
            //            {
            //                Microsoft.Cognitive.LUIS.Action act;
            //                if ((act = res.TopScoringIntent.Actions.FirstOrDefault()) != null && act.Triggered)
            //                {
            //                    //require further info from user 
            //                    if (res.DialogResponse != null && res.DialogResponse.Status == "Question")
            //                    {
            //                        choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"Clarifying Intent";
            //                        WebApiApplication.Telemetry.TrackEvent(choiceEvent);

            //                       // this.str_luis_clarification = res.DialogResponse.Prompt;
            //                        //this.bclarifyIntentWithLuis = true;
            //                        context.PrivateConversationData.SetValue(LuisHelper.STR_LUIS_DATA, res.DialogResponse);
            //                        // context.PrivateConversationData.SetValue(STR_)
            //                        context.Call(new ClarifyIntentDialog(), ClarificationComplete);
            //                        //while there is user clarification required....
            //                        //if (this.bclarifyIntentWithLuis.Value)
            //                        //{
            //                        //    context.Call(new PayFlow(), ClarificationComplete);
            //                        //    await AfterUserClarifiedIntent
            //                        //}
            //                    }
            //                    else if (res.DialogResponse != null && res.DialogResponse.Status != "Question")
            //                    {
            //                        //Go on to handle usual intents in remaining part of function below
            //                        //Handle known intents here 
            //                        //Process only if top scoring intent > 50% accuracy 
            //                        if (res.TopScoringIntent != null )
            //                        {
            //                            //Get intent name, intent score
            //                            if (res.TopScoringIntent.Name == LuisHelper.INTENT_TYPE.ENQUIRE_APPLICATION.ToDescription())
            //                            {
            //                                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = LuisHelper.INTENT_TYPE.ENQUIRE_APPLICATION.ToDescription();
            //                                WebApiApplication.Telemetry.TrackEvent(choiceEvent);
            //                                choice = 0;

            //                                if(res.Entities.Count == 0)
            //                                {
            //                                    await context.PostAsync("Please specify the nature of business e.g. setup foodstall business");
            //                                    context.Wait(AfterUserResponded);
            //                                }
            //                                else
            //                                {
            //                                    for (int i = 0; i < res.Entities.Count; ++i)
            //                                    {
            //                                        string businessType = res.Entities.ElementAt(i).Key;
            //                                        //Get backend questionaires for business type 
            //                                        context.PrivateConversationData.SetValue(LuisHelper.STR_SECTOR, res.Entities.ElementAt(i).Key);
            //                                        if (res.Entities.ElementAt(i).Value.Count() > 0)
            //                                        {
            //                                            string val = res.Entities.ElementAt(i).Value.FirstOrDefault().Value;
            //                                            context.PrivateConversationData.SetValue(LuisHelper.STR_BUSINESSINTENT, val);
            //                                        }

            //                                        context.Call(new eAdvisorDialog(), GuidedQuestionComplete);
            //                                    }
            //                                }
            //                            }
            //                            else if(res.TopScoringIntent.Name == LuisHelper.INTENT_TYPE.ENQUIRE_APPLICATION.ToDescription())
            //                            {
            //                                choiceEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = LuisHelper.INTENT_TYPE.ENQUIRE_APPLICATION.ToDescription();
            //                                WebApiApplication.Telemetry.TrackEvent(choiceEvent);
            //                                choice = 1;

            //                                await context.PostAsync("Please enter the application #");

            //                                context.Wait(AfterOpenEndedAnswer);

            ////                                PromptDialog.Text(context, AfterOpenEndedAnswer, "Please enter the application #", null, 3);
            //                            }
            //                        }
            //                        else//intent not found
            //                        {
            //                            choiceEvent.Properties[HANDLED_PROPERTY_KEY] = bool.FalseString;
            //                            WebApiApplication.Telemetry.TrackEvent(choiceEvent);
            //                            await context.PostAsync("Sorry, I didn't understand. Try one of the options above again, please.");
            //                            context.Wait(AfterUserResponded);
            //                        }
            //                    }
            //                    else//res.DialogResponse is null 
            //                    {
            //                        //TODO: should not happen, low priority
            //                        choiceEvent.Properties[HANDLED_PROPERTY_KEY] = bool.FalseString;
            //                        WebApiApplication.Telemetry.TrackEvent(choiceEvent);

            //                        await context.PostAsync("Sorry, I didn't understand.. Try one of the options above again, please.");
            //                        context.Wait(AfterUserResponded);
            //                    }
            //                }
            //                else
            //                {
            //                    if (!init)
            //                    {
            //                        init = true;
            //                        context.Wait(AfterUserResponded);
            //                    }
            //                    else
            //                    {
            //                        await context.PostAsync("Sorry, I didn't understand your intent. Please try again...");
            //                        context.Wait(AfterUserResponded);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                //TODO - low priority: Indicates all required info are available for LUIS query
            //                //Result is null, possibly errors encountered ...
            //                choiceEvent.Properties[HANDLED_PROPERTY_KEY] = bool.FalseString;
            //                WebApiApplication.Telemetry.TrackEvent(choiceEvent);
            //                await context.PostAsync("Sorry, I didn't understand... Try one of the options above again, please.");
            //                context.Wait(AfterUserResponded);
            //            }
            #endregion

            context.Wait(MainMenuResponded);
        }

        private async Task ProcessL1ExamInfo(IDialogContext context, string content)
        {
            if (content.ToUpper() == L1_MAINOPTIONS.GET_EXAM_INFO.ToDescription().ToUpper())
            {
                state = STATE.L1_GET_EXAM_INFO;
                await PostMenu(context, L2_EXAMINFOOPTIONS.GET_SYLLABUS_BY_SUBJECT, Helper.ReadConfig().Message.FirstOrDefault(x => x.ID == "L1_ExamInfoMsg").Text);
            }
            //else if(content.ToUpper() == L1_MAINOPTIONS.GET_VENUE_INFO.ToDescription().ToUpper())
            //{

            //}
            else if (content.ToUpper() == L1_MAINOPTIONS.BOOK_EXAM_SLOT.ToDescription().ToUpper())
            {
                await PostMenu(context, L2_EXAMINFOOPTIONS.GET_SYLLABUS_BY_SUBJECT, Helper.ReadConfig().Message.FirstOrDefault(x => x.ID == "L1_ExamInfoMsg").Text);
            }
            else if (content.ToUpper() == L1_MAINOPTIONS.GET_STAFF_DIRECTORY.ToDescription().ToUpper())
            {
                await PostMenu(context, L2_EXAMINFOOPTIONS.GET_SYLLABUS_BY_SUBJECT, Helper.ReadConfig().Message.FirstOrDefault(x => x.ID == "L1_ExamInfoMsg").Text);
            }
            else
            {
                #region LUIS processing 
                //LuisResult res = null;
                //try
                //{
                //    //Check Luis for intent 
                //    res = await LuisHelper.GetLuisInstance().Predict(content);
                //    if(res != null)
                //    {

                //    }
                //}
                //catch(Exception ex)
                //{
                //  Console.WriteLine("Exception " + ex.Message);
                //}
                #endregion
            }
        }

        private async Task ProcessL2Subject(IDialogContext context, string content)
        {
            if (content.ToUpper() == L1_MAINOPTIONS.GET_EXAM_INFO.ToDescription().ToUpper())
            {
                await PostMenu(context, L2_EXAMINFOOPTIONS.GET_SYLLABUS_BY_SUBJECT, Helper.ReadConfig().Message.FirstOrDefault(x => x.ID == "L1_ExamInfoMsg").Text);
            }
            //else if(content.ToUpper() == L1_MAINOPTIONS.GET_VENUE_INFO.ToDescription().ToUpper())
            //{

            //}
            else if (content.ToUpper() == L1_MAINOPTIONS.BOOK_EXAM_SLOT.ToDescription().ToUpper())
            {
                await PostMenu(context, L2_EXAMINFOOPTIONS.GET_SYLLABUS_BY_SUBJECT, Helper.ReadConfig().Message.FirstOrDefault(x => x.ID == "L1_ExamInfoMsg").Text);
            }
            else if (content.ToUpper() == L1_MAINOPTIONS.GET_STAFF_DIRECTORY.ToDescription().ToUpper())
            {
                await PostMenu(context, L2_EXAMINFOOPTIONS.GET_SYLLABUS_BY_SUBJECT, Helper.ReadConfig().Message.FirstOrDefault(x => x.ID == "L1_ExamInfoMsg").Text);
            }
            else
            {
                #region LUIS processing 
                //LuisResult res = null;
                //try
                //{
                //    //Check Luis for intent 
                //    res = await LuisHelper.GetLuisInstance().Predict(content);
                //    if(res != null)
                //    {

                //    }
                //}
                //catch(Exception ex)
                //{
                //  Console.WriteLine("Exception " + ex.Message);
                //}
                #endregion
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task PostMenu<T>(IDialogContext context, T options, string msg)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                // Can't use PromptDialog.Choice here as that *requires* the user choose from the list of choices made available.
                // What we want is to present a buttons for convenience, or let them type in a shorter (similar) response and handle
                // that in AfterChoiceMadeActivity
                // So we create a custom message here to do that.
                var menuMessage = context.MakeMessage();

                Array values = Enum.GetValues(typeof(T));
                List<string> li = new List<string>();
                foreach (Enum v in values)
                {
                    li.Add(v.ToDescription());
                }

                menuMessage.Attachments = new List<Attachment> { BotMessages.CreateMenu(context, li, msg) };
                await context.PostAsync(menuMessage);
            }
            catch (Exception ex)
            {
                await context.PostAsync(ex.Message);
            }
        }
    }
}