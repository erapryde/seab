using SeabBot.Utility;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeabBot.Utility
{
    public class BotMessages
    {
        private BotMessages() { }

        private static  BotMessages instance; 

        public static BotMessages GetInstance()
        {
            if (instance == null) instance = new BotMessages();
            return instance;
        }

        public static Attachment CreateMenu(IDialogContext context, List<string> options, string msg)
        {
            var cmsg = context.MakeMessage();
            HeroCard c = new HeroCard(null, null, msg);
            List<CardAction> buts = new List<CardAction>();
            foreach(var v in options)
            {
                buts.Add(new CardAction("postBack", v, null, v));
            }
            c.Buttons = buts;
            return c.ToAttachment();
        }
    }
}