﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SeabBot.Dialogs
{
    [Serializable]
    public class GetExamInfoDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            //Dialog dialogresponse;
            //context.PrivateConversationData.TryGetValue(LuisHelper.STR_LUIS_DATA, out dialogresponse);


            await context.PostAsync("Please enter your location ");

            context.Wait(UserResponseReceived);
        }


        public async Task UserResponseReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await result;
            // var msg = 
        }
    }
}