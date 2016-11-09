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
    public class locationDialogue : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            IMessageActivity act=context.MakeMessage();
            act.Text = "jhsdfsjdh";
            act.Attachments.Add(new Attachment("location"));

            

            await context.PostAsync(act);

            context.Wait(Response);
        }


        public async Task Response(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            string aaa = (await result).Text;
            context.Done("completed");
        }
    }
}