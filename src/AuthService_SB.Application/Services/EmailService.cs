using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AuthService_SB.Application.Interfaces;

namespace AuthService_SB.Application.Services;

public class EmailService(HttpClient httpClient, IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendEmailVerificationAsync(string email, string username, string token)
    {
        var subject = "Verify your email address";
        var verificationUrl = $"{configuration["AppSettings:FrontendUrl"]}/verify-email?token={token}";

        var body = $@"
            <h2>Welcome {username}!</h2>
            <p>Please verify your email address by clicking the link below:</p>
            <a href='{verificationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                Verify Email
            </a>
            <p>If you cannot click the link, copy and paste this URL into your browser:</p>
            <p>{verificationUrl}</p>
            <p>This link will expire in 24 hours.</p>
            <p>If you didn't create an account, please ignore this email.</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string username, string token)
    {
        var subject = "Reset your password";
        var resetUrl = $"{configuration["AppSettings:FrontendUrl"]}/reset-password?token={token}";

        var body = $@"
            <h2>Password Reset Request</h2>
            <p>Hello {username},</p>
            <p>You requested to reset your password. Click the link below to reset it:</p>
            <a href='{resetUrl}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                Reset Password
            </a>
            <p>If you cannot click the link, copy and paste this URL into your browser:</p>
            <p>{resetUrl}</p>
            <p>This link will expire in 1 hour.</p>
            <p>If you didn't request this, please ignore this email and your password will remain unchanged.</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string email, string username)
    {
        var subject = "Welcome to AuthDotnet!";

        var body = $@"
            <h2>Welcome to AuthDotnet, {username}!</h2>
            <p>Your account has been successfully verified and activated.</p>
            <p>You can now enjoy all the features of our platform.</p>
            <p>If you have any questions, feel free to contact our support team.</p>
            <p>Thank you for joining us!</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    // Uses the Resend HTTP API (port 443) instead of raw SMTP: outbound SMTP
    // (25/465/587) from Railway's shared egress IP is silently dropped by
    // Gmail's anti-abuse filtering, so MailKit connections to smtp.gmail.com
    // always time out regardless of credentials.
    private async Task SendEmailAsync(string to, string subject, string body)
    {
        var resendSettings = configuration.GetSection("ResendSettings");

        var enabled = bool.Parse(resendSettings["Enabled"] ?? "true");
        if (!enabled)
        {
            logger.LogInformation("Email disabled in configuration. Skipping send");
            return;
        }

        var apiKey = resendSettings["ApiKey"];
        var fromEmail = resendSettings["FromEmail"];
        var fromName = resendSettings["FromName"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
        {
            logger.LogError("Resend settings are not properly configured");
            throw new InvalidOperationException("Resend settings are not properly configured");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", apiKey) },
            Content = JsonContent.Create(new
            {
                from = $"{fromName} <{fromEmail}>",
                to = new[] { to },
                subject,
                html = body
            })
        };

        try
        {
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                logger.LogError("Resend API returned {StatusCode}: {Body}", response.StatusCode, responseBody);
                throw new InvalidOperationException($"Failed to send email: Resend API returned {response.StatusCode}");
            }

            logger.LogInformation("Email sent successfully");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogError(ex, "Failed to send email");
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}
