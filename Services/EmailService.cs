using MimeKit;
using System;
using System.Collections.Generic;
using MailKit.Security;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using upsa_api.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace upsa_api.Services
{
    public class EmailService: IEmailService
    {
        private readonly string SmtpHost;
        private readonly int SmtpPort;
        private readonly string SmtpAccount;
        private readonly string SmtpPassword;
        private readonly ILogger<EmailService> logger;

        public EmailService(ILogger<EmailService> _logger,
            IConfiguration configuration)
        {
            SmtpHost = configuration["email:SmtpHost"];
            SmtpAccount = configuration["email:SmtpAccountLogin"];
            SmtpPassword = configuration["email:SmtpAccountPassword"];
            _ = int.TryParse(configuration["email:SmtpPort"], out SmtpPort);
            logger = _logger;
        }

        public async Task SendMailAsync(
            IEnumerable<string> to
            , IEnumerable<string> cc
            , IEnumerable<string> bcc
            , IEnumerable<string> attachment
            , string subject
            , string body
            , int retryCount = 3)
        {
            try
            {
                var mail = BindEmailMessage(to, cc, bcc, attachment, subject, body);

                using var Smtp = new MailKit.Net.Smtp.SmtpClient();
                Smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await Smtp.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.Auto);

                if (!string.IsNullOrEmpty(SmtpAccount) && !string.IsNullOrEmpty(SmtpAccount))
                    await Smtp.AuthenticateAsync(SmtpAccount, SmtpPassword);
                
                await Smtp.SendAsync(mail);
                await Smtp.DisconnectAsync(true);
                logger.LogInformation(@$"-= Email({mail.MessageId}) para: {string.Join(";", to)}, assunto: {subject} =-");
            }
            catch (InvalidOperationException ioex)
            {
                logger.LogError(ioex, "SendMailAsync => InvalidOperationException");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SendMailAsync => Exception");
            }
        }

        private MimeMessage BindEmailMessage(
            IEnumerable<string> to
            , IEnumerable<string> cc
            , IEnumerable<string> bcc
            , IEnumerable<string> attachment
            , string subject
            , string body)
        {
            var bodyBuilder = new BodyBuilder();
            var mail = new MimeMessage
            {
                MessageId = Guid.NewGuid().ToString()
            };

            mail.From.Add(new MailboxAddress("UPSA - Sistema de Prazos", SmtpAccount));

            foreach (var address in to)
                mail.To.Add(new MailboxAddress(address));

            if (cc != null)
            {
                foreach (var address in cc)
                    mail.Cc.Add(new MailboxAddress(address));
            }

            if (bcc != null)
            {
                foreach (var address in bcc)
                    mail.Bcc.Add(new MailboxAddress(address));
            }

            if (attachment != null)
            {
                var index = 1;
                foreach (var attach in attachment)
                    _ = bodyBuilder.Attachments.Add($"{mail.MessageId}-{index++}.pdf", Convert.FromBase64String(attach));
            }

            mail.Subject = subject;
            bodyBuilder.HtmlBody = body;
            mail.Body = bodyBuilder.ToMessageBody();

            return mail;
        }
    }
}
