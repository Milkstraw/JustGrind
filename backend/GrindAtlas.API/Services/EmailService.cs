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
    Task SendNewsletterAsync(IEnumerable<string> recipients, string subject, string htmlBody);
    Task SendSupportNotificationAsync(string fromEmail, string fromName, string subject, string message);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class SmtpEmailService(IOptions<EmailSettings> opts, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailSettings _s = opts.Value;

    // ── Public methods ────────────────────────────────────────────────────────

    public Task SendWelcomeAsync(string toEmail, string displayName) =>
        SendAsync(toEmail, "Welcome to GrindAtlas!", WelcomeBody(displayName));

    public Task SendVerificationAsync(string toEmail, string displayName, string confirmUrl) =>
        SendAsync(toEmail, "Verify your GrindAtlas email", VerifyBody(displayName, confirmUrl));

    public Task SendPasswordResetAsync(string toEmail, string displayName, string resetUrl) =>
        SendAsync(toEmail, "Reset your GrindAtlas password", ResetBody(displayName, resetUrl));

    public async Task SendNewsletterAsync(IEnumerable<string> recipients, string subject, string htmlBody)
    {
        using var client = await ConnectAsync();
        foreach (var email in recipients)
        {
            try
            {
                var msg = BuildMessage(email, subject, htmlBody);
                await client.SendAsync(msg);
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

        var body = SupportBody(fromName, fromEmail, message);
        var msg  = BuildMessage(_s.SupportAddress, $"[Support] {subject}", body);
        // Reply-To so IT can reply directly to the user
        msg.ReplyTo.Add(new MailboxAddress(fromName, fromEmail));
        return SendMessageAsync(msg);
    }

    // ── Core helpers ──────────────────────────────────────────────────────────

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var msg = BuildMessage(toEmail, subject, htmlBody);
        await SendMessageAsync(msg);
    }

    private async Task SendMessageAsync(MimeMessage msg)
    {
        using var client = await ConnectAsync();
        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
        logger.LogInformation("Email sent to {To}: {Subject}", msg.To, msg.Subject);
    }

    private async Task<SmtpClient> ConnectAsync()
    {
        var client = new SmtpClient();
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
        msg.Body = new TextPart("html") { Text = htmlBody };
        return msg;
    }

    // ── Email templates ───────────────────────────────────────────────────────

    private static string WelcomeBody(string name) => $"""
        <div style="font-family:sans-serif;max-width:600px;margin:auto">
          <h2 style="color:#5d4037">Welcome to GrindAtlas, {HtmlEncode(name)}!</h2>
          <p>Your account is ready. Start tracking your grinds and discovering the perfect setting for every coffee.</p>
          <p style="color:#888;font-size:12px">Happy brewing,<br/>The GrindAtlas Team</p>
        </div>
        """;

    private static string VerifyBody(string name, string confirmUrl) => $"""
        <div style="font-family:sans-serif;max-width:600px;margin:auto">
          <h2 style="color:#5d4037">Verify your email</h2>
          <p>Hi {HtmlEncode(name)}, please click the button below to confirm your email address.</p>
          <p>
            <a href="{confirmUrl}"
               style="background:#5d4037;color:#fff;padding:12px 24px;border-radius:4px;text-decoration:none;display:inline-block">
              Verify Email
            </a>
          </p>
          <p>Or copy this link:<br/><a href="{confirmUrl}">{confirmUrl}</a></p>
          <p style="color:#888;font-size:12px">This link expires in 24 hours. If you did not create a GrindAtlas account, you can ignore this email.</p>
        </div>
        """;

    private static string ResetBody(string name, string resetUrl) => $"""
        <div style="font-family:sans-serif;max-width:600px;margin:auto">
          <h2 style="color:#5d4037">Reset your password</h2>
          <p>Hi {HtmlEncode(name)}, we received a request to reset the password for your GrindAtlas account.</p>
          <p>
            <a href="{resetUrl}"
               style="background:#5d4037;color:#fff;padding:12px 24px;border-radius:4px;text-decoration:none;display:inline-block">
              Reset Password
            </a>
          </p>
          <p>Or copy this link:<br/><a href="{resetUrl}">{resetUrl}</a></p>
          <p style="color:#888;font-size:12px">This link expires in 1 hour. If you did not request a password reset, you can safely ignore this email.</p>
        </div>
        """;

    private static string SupportBody(string fromName, string fromEmail, string message) => $"""
        <div style="font-family:sans-serif;max-width:600px;margin:auto">
          <h2 style="color:#5d4037">New Support Request</h2>
          <p><strong>From:</strong> {HtmlEncode(fromName)} &lt;{HtmlEncode(fromEmail)}&gt;</p>
          <hr/>
          <p style="white-space:pre-wrap">{HtmlEncode(message)}</p>
        </div>
        """;

    private static string HtmlEncode(string s) =>
        System.Net.WebUtility.HtmlEncode(s);
}
