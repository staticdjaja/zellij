using Microsoft.AspNetCore.Identity.UI.Services;

namespace zellij.Services
{
    public interface IEmailService : IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null);
        Task SendContactFormEmailAsync(string fromName, string fromEmail, string subject, string message, string? company = null, string? phone = null, string? projectType = null, int? projectSize = null);
        Task SendEmailConfirmationAsync(string toEmail, string confirmationLink);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }
}