namespace GrindAtlas.API.Services;

/// <summary>
/// All branded email templates for GrindAtlas.
/// Neo-brutalist design: ink (#0a0a0a) / paper (#f5f3ee) / mid (#d4d0c8),
/// Courier New monospace, asymmetric borders (1.5px top/left, 4px right/bottom).
/// </summary>
public static class EmailTemplates
{
    // ── 1. Welcome ────────────────────────────────────────────────────────────

    public static string Welcome(string name, string frontendUrl) => Layout(
        headline: "YOUR GRINDER<br/>AWAITS.",
        tagline:  "ACCOUNT REGISTERED — NGI ENABLED",
        body: $"""
            {StatRow(
                Stat("USER",      HE(name)),
                Stat("STATUS",    Pill("ACTIVE")),
                Stat("NGI RANGE", "<span style=\"font-size:14px;font-weight:700;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">0 &ndash; 100</span>")
            )}

            <p style="margin:0 0 20px;font-size:12px;line-height:1.75;letter-spacing:0.02em;color:#0a0a0a;font-family:'Courier New',Courier,monospace;">
                Welcome aboard, {HE(name)}. GrindAtlas tracks every grind you make, learns
                what you like, and bridges settings across every grinder you own &mdash; all
                through a single 0&ndash;100 scale called the <strong>NGI</strong>.
                No more sticky notes. No more guessing.
            </p>

            {Divider}
            {SectionLabel("DAY 1 CHECKLIST")}
            {ChecklistRow("[ ] LOG YOUR FIRST GRIND",    "Record a brew and start building your personal taste profile.")}
            {ChecklistRow("[ ] EXPLORE 60+ COFFEES",     "Browse the catalog and find your next favourite bag.")}
            {ChecklistRow("[ ] RUN THE GRIND ADVISOR",   "Get an NGI estimate tailored to your exact grinder &amp; coffee combo.")}
            {ChecklistRow("[ ] DIAL IN AND CONFIRM",     "After brewing, confirm your real setting so the model gets smarter.")}
            <div style="height:8px;"></div>

            {Btn("OPEN GRINDATLAS", frontendUrl)}

            {Divider}
            <p style="margin:0;font-size:10px;color:#888;font-family:'Courier New',Courier,monospace;line-height:1.6;">
                Questions? Just reply &mdash; we&rsquo;re real people who drink an unreasonable amount of coffee.
            </p>
            """
    );

    // ── 2. Email Verification ─────────────────────────────────────────────────

    public static string Verify(string name, string confirmUrl) => Layout(
        headline: "CONFIRM YOUR<br/>BEAN IDENTITY.",
        tagline:  "EMAIL VERIFICATION REQUIRED",
        body: $"""
            <p style="margin:0 0 24px;font-size:12px;line-height:1.75;letter-spacing:0.02em;color:#0a0a0a;font-family:'Courier New',Courier,monospace;">
                Hi {HE(name)} &mdash; one small step before you start dialling in.
                Tap the button below to verify your email address and unlock your full
                GrindAtlas account. The link expires in <strong>24 hours</strong>.
            </p>

            {StatRow(
                Stat("VERIFICATION", "<span style=\"font-size:11px;font-weight:700;letter-spacing:0.12em;text-transform:uppercase;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">PENDING</span>"),
                Stat("EXPIRES",      "<span style=\"font-size:11px;font-weight:700;letter-spacing:0.12em;text-transform:uppercase;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">24 HOURS</span>"),
                Stat("ACTION",       "<span style=\"font-size:11px;font-weight:700;letter-spacing:0.12em;text-transform:uppercase;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">CLICK BELOW</span>")
            )}

            {Btn("VERIFY MY EMAIL", confirmUrl)}

            {Divider}
            <p style="margin:0;font-size:10px;color:#888;font-family:'Courier New',Courier,monospace;line-height:1.6;">
                Didn&rsquo;t create a GrindAtlas account? Ignore this email safely &mdash;
                nothing will happen and no account will be activated.
            </p>
            """
    );

    // ── 3. Password Reset ─────────────────────────────────────────────────────

    public static string PasswordReset(string name, string resetUrl) => Layout(
        headline: "FORGOT YOUR<br/>COMBINATION?",
        tagline:  "PASSWORD RESET REQUESTED",
        body: $"""
            <p style="margin:0 0 24px;font-size:12px;line-height:1.75;letter-spacing:0.02em;color:#0a0a0a;font-family:'Courier New',Courier,monospace;">
                Hi {HE(name)} &mdash; we received a request to reset your GrindAtlas password.
                If that was you, click the button below. If not, your account is safe &mdash;
                ignore this email and carry on brewing.
            </p>

            {StatRow(
                Stat("REQUEST",  "<span style=\"font-size:11px;font-weight:700;letter-spacing:0.1em;text-transform:uppercase;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">PASSWORD RESET</span>"),
                Stat("EXPIRES",  "<span style=\"font-size:11px;font-weight:700;letter-spacing:0.1em;text-transform:uppercase;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">1 HOUR</span>"),
                Stat("ACCOUNT",  $"<span style=\"font-size:11px;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">{HE(name)}</span>")
            )}

            {Btn("RESET MY PASSWORD", resetUrl)}

            {Divider}
            {SectionLabel("SECURITY NOTE")}
            <p style="margin:0;font-size:10px;color:#888;font-family:'Courier New',Courier,monospace;line-height:1.6;">
                GrindAtlas will <strong>never</strong> ask for your password by email.
                If you didn&rsquo;t request a reset, reply to let us know and we&rsquo;ll
                look into it.
            </p>
            """
    );

    // ── 4. Re-engagement ──────────────────────────────────────────────────────

    public static string ReEngagement(string name, int daysSinceLastLog, string frontendUrl) => Layout(
        headline: "YOUR BEANS<br/>ARE GETTING STALE.",
        tagline:  $"LAST ACTIVITY: {daysSinceLastLog} DAYS AGO",
        body: $"""
            <p style="margin:0 0 24px;font-size:12px;line-height:1.75;letter-spacing:0.02em;color:#0a0a0a;font-family:'Courier New',Courier,monospace;">
                Hey {HE(name)} &mdash; it&rsquo;s been <strong>{daysSinceLastLog} days</strong>
                since your last grind log. That&rsquo;s at least {daysSinceLastLog} missed dials,
                {daysSinceLastLog} unlogged brews, and probably a bag of coffee that deserved
                better documentation. Your NGI data is waiting.
            </p>

            {StatRow(
                Stat("COFFEES<br/>IN CATALOG",       "<span style=\"font-size:28px;font-weight:700;letter-spacing:-0.03em;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">60+</span>"),
                Stat("INFERENCE<br/>LAYERS",         "<span style=\"font-size:28px;font-weight:700;letter-spacing:-0.03em;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">4</span>"),
                Stat("DAYS SINCE<br/>LAST LOG",      $"<span style=\"font-size:28px;font-weight:700;letter-spacing:-0.03em;font-family:'Courier New',Courier,monospace;color:#0a0a0a;\">{daysSinceLastLog}</span>")
            )}

            {Divider}
            {SectionLabel("WHAT YOU'VE BEEN MISSING")}
            {ChecklistRow("GRIND ADVISOR",        "Paste in your coffee + grinder and get an NGI estimate in seconds.")}
            {ChecklistRow("CROSS-GRINDER BRIDGE", "Already dialled in on one grinder? Transfer the setting to any other.")}
            {ChecklistRow("CONFIDENCE SCORES",    "Each estimate tells you exactly how certain the model is, and why.")}
            {ChecklistRow("SIMILARITY EXPLORER",  "Compare any two coffees side-by-side on roast, origin, and processing.")}
            <div style="height:8px;"></div>

            {Btn("LOG A GRIND NOW", frontendUrl)}

            {Divider}
            <p style="margin:0;font-size:10px;color:#888;font-family:'Courier New',Courier,monospace;line-height:1.6;">
                Don&rsquo;t want re-engagement emails? Reply with &ldquo;unsubscribe&rdquo;
                and we&rsquo;ll remove you immediately.
            </p>
            """
    );

    // ── 5. Newsletter frame ───────────────────────────────────────────────────

    /// <summary>
    /// Wraps arbitrary HTML content in the GrindAtlas newsletter shell.
    /// issueLabel example: "ISSUE #4 — MARCH 2026"
    /// </summary>
    public static string NewsletterFrame(string issueLabel, string htmlContent) => Layout(
        headline: "THE GRIND<br/>DISPATCH.",
        tagline:  issueLabel,
        body: $"""
            {htmlContent}

            {Divider}
            <p style="margin:0;font-size:10px;color:#888;font-family:'Courier New',Courier,monospace;line-height:1.6;">
                You&rsquo;re receiving this because you subscribed to GrindAtlas updates.
                Reply &ldquo;unsubscribe&rdquo; to be removed from the list.
            </p>
            """
    );

    // ── Shared layout shell ───────────────────────────────────────────────────

    private static string Layout(string headline, string tagline, string body) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width,initial-scale=1.0">
        </head>
        <body style="margin:0;padding:0;background-color:#f5f3ee;font-family:'Courier New',Courier,monospace;">
          <table width="100%" cellpadding="0" cellspacing="0" bgcolor="#f5f3ee" style="background-color:#f5f3ee;">
            <tr><td align="center" style="padding:32px 16px;">
              <table width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;">

                <!-- ═══ HEADER ═══ -->
                <tr>
                  <td bgcolor="#0a0a0a" style="background-color:#0a0a0a;padding:24px 28px;border-top:1.5px solid #0a0a0a;border-left:1.5px solid #0a0a0a;border-right:4px solid #0a0a0a;border-bottom:4px solid #0a0a0a;">
                    <p style="margin:0;font-size:9px;letter-spacing:0.18em;text-transform:uppercase;color:#d4d0c8;font-family:'Courier New',Courier,monospace;">GRINDATLAS</p>
                    <p style="margin:10px 0 0;font-size:22px;font-weight:700;letter-spacing:-0.02em;color:#f5f3ee;font-family:'Courier New',Courier,monospace;line-height:1.15;">{headline}</p>
                    <p style="margin:8px 0 0;font-size:9px;letter-spacing:0.14em;text-transform:uppercase;color:#d4d0c8;font-family:'Courier New',Courier,monospace;">{tagline}</p>
                  </td>
                </tr>

                <!-- SPACER -->
                <tr><td bgcolor="#f5f3ee" style="background-color:#f5f3ee;height:12px;"></td></tr>

                <!-- ═══ BODY ═══ -->
                <tr>
                  <td bgcolor="#ffffff" style="background-color:#ffffff;padding:28px;border-top:1.5px solid #0a0a0a;border-left:1.5px solid #0a0a0a;border-right:4px solid #0a0a0a;border-bottom:4px solid #0a0a0a;">
                    {body}
                  </td>
                </tr>

                <!-- SPACER -->
                <tr><td bgcolor="#f5f3ee" style="background-color:#f5f3ee;height:12px;"></td></tr>

                <!-- ═══ FOOTER ═══ -->
                <tr>
                  <td bgcolor="#0a0a0a" style="background-color:#0a0a0a;padding:16px 28px;border-top:1.5px solid #0a0a0a;border-left:1.5px solid #0a0a0a;border-right:4px solid #0a0a0a;border-bottom:4px solid #0a0a0a;">
                    <p style="margin:0;font-size:9px;letter-spacing:0.14em;text-transform:uppercase;color:#d4d0c8;font-family:'Courier New',Courier,monospace;">
                      GRINDATLAS &middot; PRECISION GRIND TRACKING
                    </p>
                    <p style="margin:6px 0 0;font-size:9px;color:#666;font-family:'Courier New',Courier,monospace;letter-spacing:0.03em;">
                      You&rsquo;re receiving this because you have a GrindAtlas account.
                      Questions? Reply to this email.
                    </p>
                  </td>
                </tr>

              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    // ── Component helpers ─────────────────────────────────────────────────────

    /// <summary>CTA button that renders in all major email clients.</summary>
    private static string Btn(string label, string url) => $"""
        <table cellpadding="0" cellspacing="0" style="margin:24px 0 10px;">
          <tr>
            <td style="background-color:#0a0a0a;border-top:1.5px solid #0a0a0a;border-left:1.5px solid #0a0a0a;border-right:4px solid #0a0a0a;border-bottom:4px solid #0a0a0a;">
              <a href="{url}" style="display:inline-block;padding:13px 28px;font-size:11px;font-weight:700;letter-spacing:0.12em;text-transform:uppercase;color:#f5f3ee;text-decoration:none;font-family:'Courier New',Courier,monospace;">{label} &rarr;</a>
            </td>
          </tr>
        </table>
        <p style="margin:0 0 4px;font-size:9px;color:#888;font-family:'Courier New',Courier,monospace;word-break:break-all;letter-spacing:0.02em;">
          Or copy this link: <a href="{url}" style="color:#0a0a0a;font-family:'Courier New',Courier,monospace;">{url}</a>
        </p>
        """;

    /// <summary>Three-column stat row with the paper-background panel + asymmetric border.</summary>
    private static string StatRow(string col1, string col2, string col3) => $"""
        <table width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:24px;background-color:#f5f3ee;border-top:1.5px solid #0a0a0a;border-left:1.5px solid #0a0a0a;border-right:4px solid #0a0a0a;border-bottom:4px solid #0a0a0a;">
          <tr>
            <td width="33%" style="padding:14px 16px;border-right:1px solid #d4d0c8;vertical-align:top;">{col1}</td>
            <td width="33%" style="padding:14px 16px;border-right:1px solid #d4d0c8;vertical-align:top;">{col2}</td>
            <td width="34%" style="padding:14px 16px;vertical-align:top;">{col3}</td>
          </tr>
        </table>
        """;

    /// <summary>One stat cell: label above value.</summary>
    private static string Stat(string label, string valueHtml) => $"""
        <p style="margin:0 0 6px;font-size:9px;letter-spacing:0.15em;text-transform:uppercase;color:#666;font-family:'Courier New',Courier,monospace;">{label}</p>
        {valueHtml}
        """;

    /// <summary>Inverted pill badge (ink bg, paper text).</summary>
    private static string Pill(string text) =>
        $"""<span style="display:inline-block;font-size:9px;font-weight:700;letter-spacing:0.1em;text-transform:uppercase;background-color:#0a0a0a;color:#f5f3ee;padding:3px 8px;font-family:'Courier New',Courier,monospace;">{text}</span>""";

    /// <summary>Section label (uppercase, small, muted).</summary>
    private static string SectionLabel(string text) =>
        $"""<p style="margin:0 0 10px;font-size:9px;letter-spacing:0.16em;text-transform:uppercase;color:#666;font-family:'Courier New',Courier,monospace;">{text}</p>""";

    /// <summary>Checklist row: bold title + muted description.</summary>
    private static string ChecklistRow(string title, string desc) => $"""
        <table width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:0;">
          <tr>
            <td style="padding:10px 0;border-bottom:1px solid #d4d0c8;">
              <p style="margin:0 0 3px;font-size:11px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;color:#0a0a0a;font-family:'Courier New',Courier,monospace;">{title}</p>
              <p style="margin:0;font-size:11px;color:#666;font-family:'Courier New',Courier,monospace;line-height:1.5;">{desc}</p>
            </td>
          </tr>
        </table>
        """;

    /// <summary>Full-width mid-tone divider line.</summary>
    private static string Divider =>
        """<table width="100%" cellpadding="0" cellspacing="0" style="margin:22px 0;"><tr><td style="height:1px;background-color:#d4d0c8;font-size:0;line-height:0;">&nbsp;</td></tr></table>""";

    /// <summary>HTML-encode user-provided strings to prevent injection.</summary>
    private static string HE(string s) => System.Net.WebUtility.HtmlEncode(s);
}
