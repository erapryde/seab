using Bot.Utilities;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bot.Dialogs
{
    [Serializable]
    public class LocationDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Please enter your location");
            context.Wait(UserResponseReceived);
        }


        public async Task UserResponseReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            string text = (await result).Text;

            //Processing of text 



            await context.PostAsync("");
        }
    }
}