using Bot.Utilities;
using BusinessObjects.eAdvisor;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bot.Dialogs
{
    [Serializable]
    public class eAdvisorDialog : IDialog<bool>
    {
        #region class attributes 
        List<Questionaire> qns;
        List<Message> messages; 
        Questionaire currentQuestion;

        List<Licence> licenceDtl; //list of selected licences 

#endregion  

        public async Task StartAsync(IDialogContext context)
        {
            qns = null;
            currentQuestion = null;
            licenceDtl = new List<Licence>();
            qns = new List<Questionaire>();
            messages = Helper.ReadConfig().Message.ToList();

            string sector;
            string businessintent;
            context.PrivateConversationData.TryGetValue(LuisHelper.STR_SECTOR, out sector);
            context.PrivateConversationData.TryGetValue(LuisHelper.STR_BUSINESSINTENT, out businessintent);

            //TODO: convert this into an async service call 
            BusinessIntentEnquiry query = new BusinessIntentEnquiry()
            {
                Sector = sector,
                Intent = businessintent
            };
            Questionaires res = null;
            try
            {
                //LicenceOneAPI.IeAdvisor client = WCFProxyHelper.GetInstance().GetChannel<LicenceOneAPI.IeAdvisor>();
                //res = client.GetQueries(query);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception encountered:" + ex.Message);
            }

            if (res.Queries != null && res.Queries.Count > 0)
            {
                qns.AddRange(res.Queries);
                await FieldQuestion(context);
            }
            else
            {
                Message msg = messages.FirstOrDefault(x => x.ID == "NoQuestionaireFound");
                await context.PostAsync(msg.Text);
                //Completed Q&A
                context.Done(true);
            }
        }

#pragma warning disable CS1998
        public async Task FieldQuestion(IDialogContext context)
#pragma warning restore CS1998
        {
            currentQuestion = qns.FirstOrDefault();
            qns.RemoveAt(0);

            List<string> cs = new List<string>();
            foreach (var x in currentQuestion.Choices)
            {
                cs.Add(x.SelectedChoice);
            }
            if (currentQuestion.Level == QLevel.LEVEL_1) currentQuestion.Question = messages.FirstOrDefault(x => x.ID == "Level1Questionaire").Text;
            PromptOptions<string> choices = new PromptOptions<string>(currentQuestion.Question, currentQuestion.Question, "Too many attempts failed", cs, 3);
            PromptDialog.Choice(context, AnswerReceived, choices);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task AnswerReceived(IDialogContext context, IAwaitable<string> result)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string res = await result;
            //TODO: check for level 1 or level 2
            switch (currentQuestion.Level)
            {
                case QLevel.LEVEL_1:
                    {
                        // check whether answer belongs to one of the choices 
                        qns = qns.Where(x => x.Intent == res).ToList();
                    }
                    break;
                case QLevel.LEVEL_2:
                    {
                        // check whether answer belongs to one of the choices 
                        ChoiceSelection sel = currentQuestion.Choices.FirstOrDefault(x => x.SelectedChoice == res);
                        if (sel == null) //means that choice is not entered properly by user 
                        {
                            //TODO: how to handle?

                        }
                        else if (!string.IsNullOrEmpty(sel.LicenceName)) //else check if licence name is tied to this choice 
                        {
                            string licname = currentQuestion.Choices.FirstOrDefault(x => x.SelectedChoice == res).LicenceName;
                            if (!string.IsNullOrEmpty(licname))
                            {
                                LicenceEnquiry enquiry = new LicenceEnquiry() { LicenceName = licname };
                                Licence lic = null;
                                //lic = WCFProxyHelper.GetInstance().GetChannel<LicenceOneAPI.IeAdvisor>().GetLicenceDetails(enquiry);
                                if (lic != null) licenceDtl.Add(lic);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            if (qns.Count > 0) //continue to field questions as long there are more questions 
            {
                await FieldQuestion(context);
            }
            // check whether any licences have been selected and provide the url to the licences 
            // otherwise prompt no applicable business licence 
            else
            {
                if (licenceDtl.Count == 0)
                {
                    await context.PostAsync(messages.FirstOrDefault(x => x.ID == "NoApplicableLicences").Text);
                }
                else
                {
                    StringBuilder buf = new StringBuilder(messages.FirstOrDefault(x => x.ID == "ApplyLicences").Text + Environment.NewLine);
                    foreach (var x in licenceDtl)
                    {
                        buf.AppendLine(string.Format("*{0} ({1})", x.LicenceName, x.AgencyName));
                    }

                    string title = string.Format(messages.FirstOrDefault(x => x.ID == "ClickToApplyLicence").Text, messages.FirstOrDefault(x => x.ID == "LicenceOneURL").Text);

                    var hcard = new HeroCard()
                    {
                        Images = new List<CardImage>()
                        {
                            new CardImage(messages.FirstOrDefault(x=>x.ID== "SuccessURL").Text)
                        },
                        Title = "Our Recommended Licences",
                        Text = buf.ToString(),
                        Buttons = new List<CardAction>()
                        {
                            
                            new CardAction(ActionTypes.OpenUrl, title: title, value: messages.FirstOrDefault(x => x.ID == "LicenceOneURL").Text),
                        }
                    };
                    var msg = context.MakeMessage();
                    msg.Attachments = new List<Attachment> { hcard.ToAttachment() };
                    await context.PostAsync(msg);
                }
                context.Done(true);
            }
        }
    }
}