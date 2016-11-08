using Bot.Services;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Utilities;
using System.Net.Http;
using PayPal.Api;
using Microsoft.Bot.Connector;
using System.Threading;

namespace Bot.Dialogs
{


    /*
     * 1. Bot requests a client token from server in order to initialize the client SDK
     * 2. Bot creates a payment order 
     * 3. Once the client SDK is initialized and the customer has submitted payment information, the SDK communicates that information to Braintree, which returns a payment method nonce
     * 4 You then send the payment nonce to your server
     * 5. Your server code receives the payment method nonce from your client and then uses the server SDK to create a transaction or perform other Braintree functions Credentials
     * 
     * 
     * 
     */
    [Serializable]
    public class PayFlow : IDialog<bool>
    {

        public async Task CreatePayment(APIContext apiContext, IDialogContext context)
        {
            List<FineDetails> fineDetails = (context.ConversationData.Get<List<FineDetails>>(@"notices"));

            //1. Your app or web front-end requests a client token from your server in order to initialize the client SDK
            var ctx = Bot.Utilities.Configuration.GetAPIContext();

            //2.Bot creates a payment order
            double totalAmt = 0.0;

            // ###Items
            // Items within a transaction.
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            foreach (var dtl in fineDetails)
            {
                totalAmt += dtl.Amount;
                itemList.items.Add(new Item()
                {
                    name = dtl.Description,
                    currency = "SGD",
                    price = dtl.Amount.ToString(),
                    quantity = "1"
                });

            }

            // ###Payer
            // A resource representing a Payer that funds a payment
            // Payment Method
            // as `paypal`
            var payer = new Payer() { payment_method = "paypal" };

            // ###Redirect URLS
            // These URLs will determine how the user is redirected from PayPal once they have either approved or canceled the payment.
            //var baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/PaymentWithPayPal.aspx?";
            var baseURI = System.Configuration.ConfigurationManager.AppSettings["Payment_Redirect_base_URI"];
            ResumptionCookie _resumption = null;
            context.PrivateConversationData.TryGetValue("resumption", out _resumption);
            var redirectUrl = baseURI + "resume=" + UrlToken.Encode(_resumption);
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&cancel=true",
                return_url = redirectUrl
            };

            // ###Details
            // Let's you specify details of a payment amount.
            var details = new Details()
            {
                subtotal = totalAmt.ToString()
            };

            // ###Amount
            // Let's you specify a payment amount.
            var amount = new PayPal.Api.Amount()
            {
                currency = "SGD",
                total = totalAmt.ToString()// Total must be equal to sum of shipping, tax and subtotal.
                                           //, // Total must be equal to sum of shipping, tax and subtotal.
                                           //details = details
            };

            // ###Transaction
            // A transaction defines the contract of a
            // payment - what is the payment for and who
            // is fulfilling it. 
            var transactionList = new List<PayPal.Api.Transaction>();

            // The Payment creation API requires a list of
            // Transaction; add the created `Transaction`
            // to a List
            transactionList.Add(new PayPal.Api.Transaction()
            {
                description = "Transaction description.",
                invoice_number = Common.GetRandomInvoiceNumber(),
                amount = amount,
                item_list = itemList
            });

            // ###Payment
            // A Payment Resource; create one using
            // the above types and intent as `sale` or `authorize`
            var payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a valid APIContext
            var createdPayment = payment.Create(apiContext);

            // Using the `links` provided by the `createdPayment` object, we can give the user the option to redirect to PayPal to approve the payment.
            var links = createdPayment.links.GetEnumerator();
            var urllink = string.Empty;
            while (links.MoveNext())
            {
                var link = links.Current;
                if (link.rel.ToLower().Trim().Equals("approval_url"))
                {
                    urllink = link.href;  
                }
            }
            //var msg = context.MakeMessage();
            //msg.TextFormat = TextFormatTypes.Markdown;
            //msg.Text = $"Please click [here]({urllink}) to PayPal to approve the payment...";
            await context.PostAsync($"Please click [here]({urllink}) to PayPal to approve the payment...");
            context.Wait(StoreReceiptAndFinish);
        }

        public async Task StartAsync(IDialogContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //await context.PostAsync("Click -this link- to go to Paypal and submit your payment");

            //Construct fine details 
            List<FineDetails> details = (context.ConversationData.Get<List<FineDetails> >(@"notices"));

            //1. Your app or web front-end requests a client token from your server in order to initialize the client SDK
            var apiContext = Bot.Utilities.Configuration.GetAPIContext();
            //2.Bot creates a payment order

            await CreatePayment(apiContext, context);

            //await context.PostAsync("When finished, return here and paste your Receipt #");

            
        }

        private async Task StoreReceiptAndFinish(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var receiptNumber = await result;

            // TODO: Validate entered receipt with Paypal

            WebApiApplication.Telemetry.TrackEvent("PaymentSuccessful",
                new Dictionary<string, string> { { "Receipt", receiptNumber.Text } });

            var msg = context.MakeMessage();
            msg.Text = receiptNumber.Text;

            await context.PostAsync(msg, CancellationToken.None);
            context.ConversationData.SetValue("receiptNumber", receiptNumber.Text);
            context.Done(true);
        }
    }
}