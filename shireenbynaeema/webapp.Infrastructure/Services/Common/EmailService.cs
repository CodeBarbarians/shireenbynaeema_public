namespace Application
{
    using System.Net;
    using System.Net.Mail;
    using System.Text.Json;

    using Domain;
    using Infrastructure;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using SharedServices;

    public class EmailService : IEmailService
    {
        private static readonly JsonSerializerOptions DeserializeOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IAppSettingsConfig config;
        private readonly IResponse response;
        private readonly DatabaseContext dbContext;
        private readonly string apiKey;

        public EmailService(
            IAppSettingsConfig config,
            IResponse response,
            DatabaseContext dbContext)
        {
            this.config = config;
            this.response = response;
            this.dbContext = dbContext;
            this.apiKey = config.SendGrid?.ApiKey ?? string.Empty;
        }

        private SmtpConfig? GetSmtpConfig()
        {
            var setting = dbContext.Settings.FirstOrDefault(s => s.Key == "SmtpConfig");
            if (setting == null || string.IsNullOrWhiteSpace(setting.Value)) return null;
            try { return JsonSerializer.Deserialize<SmtpConfig>(setting.Value, DeserializeOptions); }
            catch { return null; }
        }

        private async Task<bool> SendViaSmtp(MailMessage mailMessage)
        {
            var smtpConfig = GetSmtpConfig();
            if (smtpConfig == null || !smtpConfig.UseSmtp || string.IsNullOrWhiteSpace(smtpConfig.Host))
                return false;

            try
            {
                using var client = new SmtpClient(smtpConfig.Host, smtpConfig.Port)
                {
                    EnableSsl = smtpConfig.EnableSsl,
                    Credentials = new NetworkCredential(smtpConfig.Username, smtpConfig.Password),
                    Timeout = 30000,
                };
                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendViaSendGrid(
            string toEmail,
            string subject,
            string plainText,
            string htmlContent,
            string fromEmail,
            string fromName,
            List<(string FileName, string Base64File)>? attachments,
            List<string>? ccs,
            List<string>? replytos)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) return false;

            try
            {
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, fromName);
                var toEmails = toEmail.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                      .Select(x => x.ToLowerInvariant()).Distinct().ToList();
                var toList = toEmails.Select(e => new EmailAddress(e)).ToList();

                var msg = MailHelper.CreateSingleEmail(from, toList.First(), subject, plainText, htmlContent);

                foreach (var to in toList.Skip(1)) msg.AddTo(to);

                if (ccs != null && ccs.Count != 0)
                {
                    var ccList = ccs.Select(c => c.Trim().ToLowerInvariant())
                                    .Where(c => !string.IsNullOrWhiteSpace(c))
                                    .Distinct().Except(toEmails, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var cc in ccList) msg.AddCc(new EmailAddress(cc));
                }

                if (replytos != null && replytos.Count != 0)
                {
                    msg.ReplyTos = replytos.Select(c => c.Trim().ToLowerInvariant())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct().Select(x => new EmailAddress { Email = x, Name = x }).ToList();
                }

                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                        msg.AddAttachment(attachment.FileName, attachment.Base64File);
                }

                var resp = await client.SendEmailAsync(msg);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IResponse> SendEmail(
            string toEmail,
            string subject,
            string plainText,
            string htmlContent,
            string fromKey = "PwdReset",
            string? fromNameOverride = null,
            List<(string FileName, string Base64File)>? attachments = null,
            string? fromEmailOverride = null,
            List<string>? ccs = null,
            List<string>? replytos = null)
        {
            var smtpConfig = GetSmtpConfig();
            string fromEmail;
            string fromName = fromNameOverride ?? "Shireen by Naeema";

            if (smtpConfig != null && smtpConfig.UseSmtp && !string.IsNullOrWhiteSpace(smtpConfig.FromEmail))
            {
                fromEmail = fromEmailOverride ?? smtpConfig.FromEmail;
                fromName = fromNameOverride ?? smtpConfig.FromName;
            }
            else
            {
                fromEmail = string.IsNullOrWhiteSpace(fromEmailOverride)
                    ? this.config.SendGrid?.FromEmail?.GetType()
                        .GetProperty(fromKey)?.GetValue(this.config.SendGrid.FromEmail)?.ToString() ?? string.Empty
                    : fromEmailOverride;
            }

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new InvalidOperationException($"From email not configured for key '{fromKey}'.");

            var toEmails = toEmail.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                  .Select(x => x.ToLowerInvariant()).Distinct().ToList();

            bool sent = false;

            // Try SMTP first
            if (smtpConfig != null && smtpConfig.UseSmtp && !string.IsNullOrWhiteSpace(smtpConfig.Host))
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlContent,
                    IsBodyHtml = true,
                };
                foreach (var email in toEmails) mailMessage.To.Add(email);
                if (ccs != null) foreach (var cc in ccs.Where(c => !string.IsNullOrWhiteSpace(c))) mailMessage.CC.Add(cc);
                if (replytos != null && replytos.Count > 0) mailMessage.ReplyToList.Add(new MailAddress(replytos.First()));

                sent = await SendViaSmtp(mailMessage);
            }

            // Fallback to SendGrid
            if (!sent)
            {
                sent = await SendViaSendGrid(toEmail, subject, plainText, htmlContent, fromEmail, fromName, attachments, ccs, replytos);
            }

            this.response.Message = sent ? Constants.Email.SentSuccess : "Email sending failed";
            this.response.IsSuccess = sent;

            if (sent)
            {
                var emailLogs = toEmails.Select(email => new EmailLog
                {
                    Id = Guid.NewGuid(),
                    ToEmail = email,
                    FromEmail = fromEmail,
                    Subject = subject,
                    CCs = ccs != null ? string.Join(",", ccs) : null,
                }).ToList();

                await dbContext.EmailLogs.AddRangeAsync(emailLogs);
                await dbContext.SaveChangesAsync();
            }

            return this.response;
        }
    }
}
