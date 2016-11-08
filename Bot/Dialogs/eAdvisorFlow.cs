using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Bot.Dialogs
{
    public static class eAdvisorFlow
    {
        public static async Task HandleBusinessIntent(CancellationToken token)
        {
            // since this is an externally-triggered event, this is the composition root
            // find the dependency injection container
            var container = Global.FindContainer();

            await HandleAlarm(container, alarm, now, token);
        }

        public static async Task HandleAlarm(Alarm alarm, DateTime now, CancellationToken token)
        {
            // since this is an externally-triggered event, this is the composition root
            // find the dependency injection container
            var container = Global.FindContainer();

            await HandleAlarm(container, alarm, now, token);
        }

        public static async Task HandleAlarm(ILifetimeScope container, Alarm alarm, DateTime now, CancellationToken token)
        {
            // the ResumptionCookie has the "key" necessary to resume the conversation
            var message = alarm.Cookie.GetMessage();
            // we instantiate our dependencies based on an IMessageActivity implementation
            using (var scope = DialogModule.BeginLifetimeScope(container, message))
            {
                // find the bot data interface and load up the conversation dialog state
                var botData = scope.Resolve<IBotData>();
                await botData.LoadAsync(token);

                // resolve the dialog stack
                var stack = scope.Resolve<IDialogStack>();
                // make a dialog to push on the top of the stack
                var child = scope.Resolve<AlarmRingDialog>(TypedParameter.From(alarm.Title));
                // wrap it with an additional dialog that will restart the wait for
                // messages from the user once the child dialog has finished
                var interruption = child.Void<object, IMessageActivity>();

                try
                {
                    // put the interrupting dialog on the stack
                    stack.Call(interruption, null);
                    // start running the interrupting dialog
                    await stack.PollAsync(token);
                }
                finally
                {
                    // save out the conversation dialog state
                    await botData.FlushAsync(token);
                }
            }
        }
    }
}