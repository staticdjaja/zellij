using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using zellij.Models;

namespace zellij.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendEmailAsync(email, subject, htmlMessage, null);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = htmlContent;
                if (!string.IsNullOrEmpty(plainTextContent))
                {
                    bodyBuilder.TextBody = plainTextContent;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendContactFormEmailAsync(string fromName, string fromEmail, string subject, string message, string? company = null, string? phone = null, string? projectType = null, int? projectSize = null)
        {
            var htmlTemplate = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; margin-bottom: 30px; }}
                        .content {{ line-height: 1.6; color: #333; }}
                        .field {{ margin-bottom: 15px; }}
                        .label {{ font-weight: bold; color: #555; }}
                        .value {{ margin-left: 10px; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 14px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>New Contact Form Submission</h1>
                            <p>CESARO Website</p>
                        </div>
                        <div class='content'>
                            <div class='field'>
                                <span class='label'>From:</span>
                                <span class='value'>{fromName} ({fromEmail})</span>
                            </div>
                            {(!string.IsNullOrEmpty(company) ? $"<div class='field'><span class='label'>Company:</span><span class='value'>{company}</span></div>" : "")}
                            {(!string.IsNullOrEmpty(phone) ? $"<div class='field'><span class='label'>Phone:</span><span class='value'>{phone}</span></div>" : "")}
                            {(!string.IsNullOrEmpty(projectType) ? $"<div class='field'><span class='label'>Project Type:</span><span class='value'>{projectType}</span></div>" : "")}
                            {(projectSize.HasValue ? $"<div class='field'><span class='label'>Project Size:</span><span class='value'>{projectSize} sq ft</span></div>" : "")}
                            <div class='field'>
                                <span class='label'>Subject:</span>
                                <span class='value'>{subject}</span>
                            </div>
                            <div class='field'>
                                <span class='label'>Message:</span>
                                <div style='margin-top: 10px; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #007bff; border-radius: 4px;'>
                                    {message.Replace("\n", "<br>")}
                                </div>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>This message was sent from the CESARO contact form on {DateTime.Now:MMM dd, yyyy 'at' HH:mm}</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(_emailSettings.AdminEmail, $"Contact Form: {subject}", htmlTemplate);

            // Send confirmation email to the user
            var confirmationTemplate = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; margin-bottom: 30px; }}
                        .content {{ line-height: 1.6; color: #333; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 14px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Thank You for Contacting Us!</h1>
                            <p>CESARO</p>
                        </div>
                        <div class='content'>
                            <p>Dear {fromName},</p>
                            <p>Thank you for reaching out to us. We have received your message and will get back to you within 24 hours.</p>
                            <p><strong>Your message details:</strong></p>
                            <ul>
                                <li><strong>Subject:</strong> {subject}</li>
                                {(!string.IsNullOrEmpty(projectType) ? $"<li><strong>Project Type:</strong> {projectType}</li>" : "")}
                                {(projectSize.HasValue ? $"<li><strong>Project Size:</strong> {projectSize} sq ft</li>" : "")}
                            </ul>
                            <p>Our team will review your inquiry and respond promptly with the information you requested.</p>
                            <p>Best regards,<br>The CESARO Team</p>
                        </div>
                        <div class='footer'>
                            <p>CESARO | +212 522-XXX-XXX | info@zellijmarble.ma</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(fromEmail, "Thank you for contacting CESARO", confirmationTemplate);
        }

        public async Task SendEmailConfirmationAsync(string toEmail, string confirmationLink)
        {
            var htmlTemplate = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; margin-bottom: 30px; }}
                        .content {{ line-height: 1.6; color: #333; }}
                        .button {{ display: inline-block; padding: 12px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 14px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Confirm Your Email Address</h1>
                            <p>CESARO</p>
                        </div>
                        <div class='content'>
                            <p>Welcome to CESARO!</p>
                            <p>Please confirm your email address by clicking the button below:</p>
                            <div style='text-align: center;'>
                                <a href='{confirmationLink}' class='button'>Confirm Email Address</a>
                            </div>
                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px;'>{confirmationLink}</p>
                            <p>If you didn't create an account with us, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>CESARO | +212 522-XXX-XXX | info@zellijmarble.ma</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, "Confirm your email address - CESARO", htmlTemplate);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var htmlTemplate = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; margin-bottom: 30px; }}
                        .content {{ line-height: 1.6; color: #333; }}
                        .button {{ display: inline-block; padding: 12px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 14px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Reset Your Password</h1>
                            <p>CESARO</p>
                        </div>
                        <div class='content'>
                            <p>You recently requested to reset your password for your account. Click the button below to reset it:</p>
                            <div style='text-align: center;'>
                                <a href='{resetLink}' class='button'>Reset Password</a>
                            </div>
                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px;'>{resetLink}</p>
                            <p>If you didn't request a password reset, please ignore this email or contact support if you have questions.</p>
                            <p>This link will expire in 24 hours for security reasons.</p>
                        </div>
                        <div class='footer'>
                            <p>CESARO | +212 522-XXX-XXX | info@zellijmarble.ma</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, "Reset your password - CESARO", htmlTemplate);
        }
    }
}