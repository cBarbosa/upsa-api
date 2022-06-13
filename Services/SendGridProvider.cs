using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace upsa_api.Services
{
    public class SendGridProvider
    {
        public SendGridEmailSenderOptions Options { get; set; }

        public SendGridProvider(
            IOptions<SendGridEmailSenderOptions> options
            )
        {
            Options = options.Value;
        }

        public async Task<System.Net.HttpStatusCode> SendEmailAsync(
            IEnumerable<string> tos,
            IEnumerable<string> ccs,
            IEnumerable<string> bccs,
            string subject,
            string message)
        {
            var result = await Execute(Options.ApiKey, subject, message, tos, ccs, bccs);
            //result.StatusCode == System.Net.HttpStatusCode
            return result.StatusCode;
        }

        private async Task<Response> Execute(
            string apiKey,
            string subject,
            string message,
            IEnumerable<string> tos,
            IEnumerable<string> ccs,
            IEnumerable<string> bccs)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(Options.SenderEmail, Options.SenderName),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            foreach (var email in tos)
            {
                msg.AddTo(new EmailAddress(email));
            }

            if (ccs != null)
            {
                foreach (var cc in ccs)
                {
                    msg.AddBcc(new EmailAddress(cc));
                }
            }

            if (bccs != null)
            {
                foreach (var bcc in bccs)
                {
                    msg.AddBcc(new EmailAddress(bcc));
                }
            }

            // disable tracking settings
            // ref.: https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(true, false);
            msg.SetOpenTracking(true);
            msg.SetGoogleAnalytics(false);
            msg.SetSubscriptionTracking(false);
            //msg.SetTemplateId("d-556db1e68229482693f72e1ce9b14dcc");
            //msg.SetTemplateData(new { first_name = "Charles Barbosa" });
            msg.AddCategory("UPSA");

            var result = await client.SendEmailAsync(msg);
            return result;
        }
    }

    public class SendGridEmailSenderOptions
    {
        public string ApiKey { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
    }
}