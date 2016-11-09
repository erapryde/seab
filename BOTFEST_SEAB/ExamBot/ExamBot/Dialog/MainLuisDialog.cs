using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExamBot.Dialog
{
    [LuisModel("6dcb9387-f3f9-4613-9332-f52b226bda97", "f0264918b26f4f838bac2123e9092768")]
    [Serializable]
    public class MainLuisDialog : LuisDialog<object>
    {

        public const string E_EXAM_EXAM_SUBJECT = "ExamSubject";

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I can't understand your message at the moment, sorry!");
            context.Wait(MessageReceived);
        }

        [LuisIntent("getExamInfo")]
        public async Task getExamInfo(IDialogContext context, LuisResult result)
        {
            string message = "";

            //find the specific intention
            bool isEntityExist = result.Entities.Count > 0;
            


            //process
            if (!isEntityExist)
            {
                message = "Paiseh, simi ar?";
            }else
            {
                

                message = "Paiseh, buay hiaw... Train me pls...";
            }


            
            //send back response
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }


    }
}