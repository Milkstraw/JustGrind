using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GrindAtlas.API.Services;

// ── Settings ─────────────────────────────────────────────────────────────────

public class EmailSettings
{
    public string Host            { get; set; } = "smtp.gmail.com";
    public int    Port            { get; set; } = 587;
    public bool   UseStartTls     { get; set; } = true;
    public string Username        { get; set; } = "";
    /// <summary>
    /// Set via env var EmailSettings__Password — never store in source control.
    /// For Gmail, use an App Password (Google Account → Security → App Passwords).
    /// </summary>
    public string Password        { get; set; } = "";
    public string FromName        { get; set; } = "GrindAtlas";
    /// <summary>Address where contact/support emails are delivered.</summary>
    public string SupportAddress  { get; set; } = "";
    /// <summary>Base URL of the Angular app, used to build confirmation/reset links.</summary>
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IEmailService
{
    Task SendWelcomeAsync(string toEmail, string displayName);
    Task SendVerificationAsync(string toEmail, string displayName, string confirmUrl);
    Task SendPasswordResetAsync(string toEmail, string displayName, string resetUrl);
    Task SendReEngagementAsync(string toEmail, string displayName, int daysSinceLastLog);
    Task SendNewsletterAsync(IEnumerable<string> recipients, string subject, string htmlBody);
    Task SendSupportNotificationAsync(string fromEmail, string fromName, string subject, string message);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class SmtpEmailService(IOptions<EmailSettings> opts, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailSettings _s = opts.Value;

    // ── Public methods ────────────────────────────────────────────────────────

    public Task SendWelcomeAsync(string toEmail, string displayName) =>
        SendAsync(toEmail, "Welcome to GrindAtlas — your grinder awaits",
            EmailTemplates.Welcome(displayName, _s.FrontendBaseUrl));

    public Task SendVerificationAsync(string toEmail, string displayName, string confirmUrl) =>
        SendAsync(toEmail, "Confirm your GrindAtlas email address",
            EmailTemplates.Verify(displayName, confirmUrl));

    public Task SendPasswordResetAsync(string toEmail, string displayName, string resetUrl) =>
        SendAsync(toEmail, "GrindAtlas password reset",
            EmailTemplates.PasswordReset(displayName, resetUrl));

    public Task SendReEngagementAsync(string toEmail, string displayName, int daysSinceLastLog) =>
        SendAsync(toEmail, $"Your beans are getting stale — it's been {daysSinceLastLog} days",
            EmailTemplates.ReEngagement(displayName, daysSinceLastLog, _s.FrontendBaseUrl));

    public async Task SendNewsletterAsync(IEnumerable<string> recipients, string subject, string htmlBody)
    {
        using var client = await ConnectAsync();
        foreach (var email in recipients)
        {
            try
            {
                await client.SendAsync(BuildMessage(email, subject, htmlBody));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send newsletter to {Email}", email);
            }
        }
        await client.DisconnectAsync(true);
    }

    public Task SendSupportNotificationAsync(string fromEmail, string fromName, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(_s.SupportAddress))
        {
            logger.LogWarning("SupportAddress is not configured — contact email not sent.");
            return Task.CompletedTask;
        }

        var html = SupportBody(fromName, fromEmail, message);
        var msg  = BuildMessage(_s.SupportAddress, $"[Support] {subject}", html);
        msg.ReplyTo.Add(new MailboxAddress(fromName, fromEmail));
        return SendMessageAsync(msg);
    }

    // ── Core helpers ──────────────────────────────────────────────────────────

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
        => await SendMessageAsync(BuildMessage(toEmail, subject, htmlBody));

    private async Task SendMessageAsync(MimeMessage msg)
    {
        using var client = await ConnectAsync();
        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
        logger.LogInformation("Email sent to {To}: {Subject}", msg.To, msg.Subject);
    }

    private async Task<SmtpClient> ConnectAsync()
    {
        var client  = new SmtpClient();
        var options = _s.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;
        await client.ConnectAsync(_s.Host, _s.Port, options);
        await client.AuthenticateAsync(_s.Username, _s.Password);
        return client;
    }

    private MimeMessage BuildMessage(string toEmail, string subject, string htmlBody)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_s.FromName, _s.Username));
        msg.To.Add(MailboxAddress.Parse(toEmail));
        msg.Subject = subject;
        msg.Body    = new TextPart("html") { Text = htmlBody };
        return msg;
    }

    // ── Support email (plain template, not in EmailTemplates) ─────────────────

    private static string SupportBody(string fromName, string fromEmail, string message) =>
        $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head><meta charset="UTF-8"></head>
        <body style="margin:0;padding:0;background-color:#f5f3ee;font-family:'Courier New',Courier,monospace;">
          <table width="100%" cellpadding="0" cellspacing="0" bgcolor="#f5f3ee" style="background-color:#f5f3ee;">
            <tr><td align="center" style="padding:32px 16px;">
              <table width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;">
                <tr>
                  <td bgcolor="#0a0a0a" style="background-color:#0a0a0a;padding:24px 28px;border-top:1.5px solid #0a0a0a;border-left:1.5px solid #0a0a0a;border-right:4px solid #0a0a0a;border-bottom:4px solid #0a0a0a;">
                    <p style="margin:0;font-size:9px;letter-spacing:0.18em;text-transform:uppercase;color:#d4d0c8;font-family:'Courier New',Courier,monospace;">GRINDATLAS &middot; SUPPORT</p>
                    <p style="margin:10px 0 0;font-size:22px;font-weight:700;letter-spacing:-0.02em;color:#f5f3ee;font-family:'Courier New',Courier,monospace;line-height:1.15;">NEW SUPPORT<br/>REQUEST.</p>
                  </td>
                </tr>
                <tr><td bgcolor="#f5f3ee" style="background-color:#f5f3ee;height:12px;"></td></tr>
                <tr>
                  <td bgcolor="#ffffff" style="background-color:#ffffff;padding:28px;border-top:1.5px solid #0a0a0a;border-left:1.5px solid #0a0a0a;border-right:4px solid #0a0a0a;border-bottom:4px solid #0a0a0a;">
                    <table width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:24px;background-color:#f5f3ee;border-top:1.5px solid #0a0a0a;border-left:1.5px solid #0a0a0a;border-right:4px solid #0a0a0a;border-bottom:4px solid #0a0a0a;">
                      <tr>
                        <td width="50%" style="padding:14px 16px;border-right:1px solid #d4d0c8;vertical-align:top;">
                          <p style="margin:0 0 6px;font-size:9px;letter-spacing:0.15em;text-transform:uppercase;color:#666;font-family:'Courier New',Courier,monospace;">FROM</p>
                          <p style="margin:0;font-size:13px;font-weight:700;color:#0a0a0a;font-family:'Courier New',Courier,monospace;">{{System.Net.WebUtility.HtmlEncode(fromName)}}</p>
                          <p style="margin:3px 0 0;font-size:11px;color:#666;font-family:'Courier New',Courier,monospace;">{{System.Net.WebUtility.HtmlEncode(fromEmail)}}</p>
                        </td>
                        <td width="50%" style="padding:14px 16px;vertical-align:top;">
                          <p style="margin:0 0 6px;font-size:9px;letter-spacing:0.15em;text-transform:uppercase;color:#666;font-family:'Courier New',Courier,monospace;">REPLY-TO</p>
                          <p style="margin:0;font-size:11px;color:#0a0a0a;font-family:'Courier New',Courier,monospace;">Hit Reply to respond directly to the sender.</p>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:0 0 10px;font-size:9px;letter-spacing:0.15em;text-transform:uppercase;color:#666;font-family:'Courier New',Courier,monospace;">MESSAGE</p>
                    <p style="margin:0;font-size:12px;line-height:1.75;color:#0a0a0a;font-family:'Courier New',Courier,monospace;white-space:pre-wrap;">{{System.Net.WebUtility.HtmlEncode(message)}}</p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
