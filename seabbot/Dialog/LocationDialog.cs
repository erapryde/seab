using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SeabBot.Dialog
{
    [Serializable]
    public class LocationDialog : IDialog<bool>
    {
        public async Task StartAsync(IDialogContext context)
        {
            try
            {
            //    var cmsg = context.MakeMessage();
            //    cmsg.Attachments = new List<Attachment>()
            //    {
            //        new Attachment()
            //        {
            //            ContentType = "location",
            //        }
            //    };

            //    cmsg.ChannelData = "
            //    facebook:
            //        {
            //            quick_replies:
            //    [{
            //                content_type: "location"
            //    }]
            //}
            //    });
            //    session.send(replyMessage);
            //    cmsg.Text = "Send your location";
            //    await context.PostAsync(cmsg);
            }
            catch(Exception ex)
            {
                await context.PostAsync("Crap, exception found! ");
            }
            context.Wait(UserResponseReceived);
        }

        public async Task UserResponseReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var res = (await result);
                context.Done(true);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}