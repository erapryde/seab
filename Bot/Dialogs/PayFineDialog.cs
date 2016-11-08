using Bot.Services;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot.Dialogs.PayFine
{
    [Serializable]
    public class PayFineDialog : IDialog<bool>
    {
        private const string NOTICES_STATE_BAG_KEY = @"notices";

        private static readonly Regex NOTICE_VALIDATOR = new Regex("^[A-Za-z0-9]{10}$", RegexOptions.Compiled);
        private static readonly List<string> EXIT_COMMANDS = new List<string> { "start over", "done", "exit", "quit", "reset" };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StartAsync(IDialogContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            await PromptForEntry(context,
                @"I'd be happy to help you pay a fine!
Please enter your notice or vehicle number.

If at any time you wish to start over, you can type 'start over' or 'done'");
        }

        private async Task AfterNoticeEntryPromptActivity(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var content = await result;
            var resumption = new ResumptionCookie(content);
            context.PrivateConversationData.SetValue("resumption", resumption);
            await AfterNoticeEntryPrompt(context, content.Text);
        }
        private async Task AfterNoticeEntryPromptString(IDialogContext context, IAwaitable<string> result) => await AfterNoticeEntryPrompt(context, await result);
        private async Task AfterNoticeEntryPrompt(IDialogContext context, string result)
        {
            var userEntry = result.Trim();  // trim because might be copy/pasting from elsewhere and have whitespace on either end

            const string SELECTED_CHOICE_PROPERTY_KEY = @"SelectedChoice";
            const string HANDLED_PROPERTY_KEY = @"Handled";
            EventTelemetry userEntryEvent = new EventTelemetry(@"PayFineDialog-User Choice");
            userEntryEvent.Properties.Add(@"User Text", result);
            userEntryEvent.Properties.Add(HANDLED_PROPERTY_KEY, bool.TrueString);
            userEntryEvent.Properties.Add(SELECTED_CHOICE_PROPERTY_KEY, @"Unknown");

            int numFines = NumFinesToPay(context);
            userEntryEvent.Metrics.Add("NumFines", numFines);

            if (userEntry.Equals("pay", StringComparison.OrdinalIgnoreCase)
                | userEntry.Equals("pay now", StringComparison.OrdinalIgnoreCase))
            {
                userEntryEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"PayFines";
                WebApiApplication.Telemetry.TrackEvent(userEntryEvent);

                if (numFines > 0)
                {
                    context.Call(new PayFlow(), PaymentComplete);
                }
                else
                {
                    await PromptForEntry(context,
                        @"You haven't entered any fines to pay. Type a notice or vehicle # to look up and pay");
                }
            }
            else if (EXIT_COMMANDS.Contains(userEntry.ToLowerInvariant()))
            {
                userEntryEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = "Done";
                WebApiApplication.Telemetry.TrackEvent(userEntryEvent);

                BeDone(context, false);
            }
            else if (userEntry.ToLowerInvariant().Contains("show")
                | userEntry.ToLowerInvariant().Contains("list"))
            {
                userEntryEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = "ShowOrList";
                WebApiApplication.Telemetry.TrackEvent(userEntryEvent);

                if (numFines > 0)
                {
                    await PromptForEntry(context, $@"So far, you're set to pay:

{BuildNoticesList(context)}

Total: {FineTotalString(context)}");
                }
                else
                {
                    await PromptForEntry(context,
                        @"You haven't yet entered any fines to pay. Enter a notice or vehicle # now, or type 'done' to go back to the main menu.");
                }
            }
            else
            {
                if (NOTICE_VALIDATOR.IsMatch(userEntry))
                {
                    userEntryEvent.Properties[SELECTED_CHOICE_PROPERTY_KEY] = @"AddFine";

                    WebApiApplication.Telemetry.TrackEvent(userEntryEvent);

                    // Make sure the user hasn't already entered this fine
                    List<FineDetails> listSoFar;
                    if (context.ConversationData.TryGetValue(NOTICES_STATE_BAG_KEY, out listSoFar))
                    {
                        if (listSoFar.Any(i => i.Id.Equals(userEntry, StringComparison.OrdinalIgnoreCase)))
                        {
                            userEntryEvent.Properties[@"Success"] = bool.FalseString;
                            userEntryEvent.Properties[@"ErrorReason"] = @"DuplicateNotice";
                            WebApiApplication.Telemetry.TrackEvent(userEntryEvent);

                            await PromptForEntry(context,
                                @"You're already set to pay that fine. Check the number and try again.");

                            return;
                        }
                    }

                    IFineLookupService lookupService = new MockLookupService();
                    try
                    {
                        var details = lookupService.LookUpFine(userEntry);
                        context.ConversationData.SetValue("lookedUpDetails", details);

                        PromptDialog.Confirm(context, AfterConfirm_CorrectFineFound,
                            $@"Is this the fine you're looking for?

""{details.Description}"" - {details.Amount.ToString("C", CultureInfo.CurrentUICulture)}");
                    }
                    catch (ArgumentException)
                    {
                        await PromptForEntry(context,
                            $"No notice or vehicle # was found matching {userEntry}. Please check the number and try again.");
                    }
                }
                else
                {
                    userEntryEvent.Properties[@"Handled"] = bool.FalseString;
                    WebApiApplication.Telemetry.TrackEvent(userEntryEvent);

                    WebApiApplication.Telemetry.TrackEvent("PayFineDialog-Invalid notice #",
                        new Dictionary<string, string> { { "EnteredNum", userEntry } });
                    await PromptForEntry(context, @"That doesn't look like a valid notice or vehicle #. Please try again.");
                }
            }
        }

        private async Task AfterConfirm_CorrectFineFound(IDialogContext context, IAwaitable<bool> confirmation)
        {
            FineDetails details = context.ConversationData.Get<FineDetails>("lookedUpDetails");

            if (await confirmation)
            {
                List<FineDetails> listSoFar;
                if (context.ConversationData.TryGetValue(NOTICES_STATE_BAG_KEY, out listSoFar))
                {
                    listSoFar.Add(details);
                }
                else
                {
                    listSoFar = new List<FineDetails> { details };
                }

                context.ConversationData.SetValue(NOTICES_STATE_BAG_KEY, listSoFar);

                await PromptForEntry(context,
                    $@"Got it.
Here are the notices you've entered so far:

{BuildNoticesList(listSoFar)}

Enter another notice/vehicle # or choose to pay now (Total: {FineTotalString(listSoFar)})");

                WebApiApplication.Telemetry.TrackEvent("PayFineDialog-notice added to list",
                    new Dictionary<string, string> { { "Entered Notice", JsonConvert.SerializeObject(details) } });
            }
            else
            {
                WebApiApplication.Telemetry.TrackEvent("PayFineDialog-incorrect notice found",
                    new Dictionary<string, string> { { "Entered Notice", JsonConvert.SerializeObject(details) } });

                var sorryMsg = new StringBuilder(@"Sorry about that. Check to make sure you typed the notice/vehicle # in correctly and try again");
                if (NumFinesToPay(context) > 0)
                {
                    sorryMsg.Append("or type 'pay' to pay the fines you've entered so far");
                }

                sorryMsg.Append(".");

                await PromptForEntry(context, sorryMsg);
            }
        }

        private async Task PromptForEntry(IDialogContext context, StringBuilder msgBuilder) => await PromptForEntry(context, msgBuilder?.ToString());
        private async Task PromptForEntry(IDialogContext context, string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) throw new ArgumentNullException(nameof(msg));

            if (NumFinesToPay(context) > 0)
            {
                // Can't use PromptDialog.Choice here as that *requires* the user choose from the list of choices made available.
                // What we want is to present a Pay button all the time, or let them put in another vehicle/notice number and go through
                // the Add Notice flow again
                // So we create a custom message here to do that.
                var payMessage = context.MakeMessage();
                payMessage.Text = msg;
                payMessage.TextFormat = "plain";

                var card = new HeroCard();
                card.Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, @"Pay Now", value: @"Pay Now") };
                payMessage.Attachments = new List<Attachment> { card.ToAttachment() };

                await context.PostAsync(payMessage);
                context.Wait(AfterNoticeEntryPromptActivity);
            }
            else
            {
                PromptDialog.Text(context, AfterNoticeEntryPromptString, msg);
            }
        }

        private string BuildNoticesList(IDialogContext context)
        {
            List<FineDetails> listSoFar;
            if (context.ConversationData.TryGetValue(NOTICES_STATE_BAG_KEY, out listSoFar))
            {
                return BuildNoticesList(listSoFar);
            }

            return string.Empty;
        }

        private string BuildNoticesList(IEnumerable<FineDetails> fromCollection) =>
                string.Join("\n", fromCollection.Select(i => $"{i.Id} - {i.Amount.ToString("C", CultureInfo.CurrentUICulture)}"));

        private string FineTotalString(IDialogContext context)
        {
            List<FineDetails> listSoFar;
            if (context.ConversationData.TryGetValue(NOTICES_STATE_BAG_KEY, out listSoFar))
            {
                return FineTotalString(listSoFar);
            }
            else
            {
                return string.Empty;
            }
        }

        private string FineTotalString(IEnumerable<FineDetails> fromCollection) => fromCollection.Sum(i => i.Amount).ToString("C", CultureInfo.CurrentUICulture);

        private int NumFinesToPay(IDialogContext context)
        {
            IList<FineDetails> fines;
            return context.ConversationData.TryGetValue(NOTICES_STATE_BAG_KEY, out fines) ? fines.Count : 0;
        }

        private async Task PaymentComplete(IDialogContext context, IAwaitable<bool> result)
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

        private void BeDone(IDialogContext context, bool result = true)
        {
            context.ConversationData.RemoveValue(NOTICES_STATE_BAG_KEY);
            context.Done(result);
        }
    }
}