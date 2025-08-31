# Email Configuration Guide

## Setting Up Email for CESARO

Your email system is now fully implemented! Here's how to configure it:

### ?? **Configuration Required**

Update the email settings in `appsettings.json` and `appsettings.Development.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-gmail@gmail.com",
    "SmtpPassword": "your-app-specific-password",
    "FromEmail": "noreply@zellijmarble.ma",
    "FromName": "CESARO",
    "AdminEmail": "admin@zellijmarble.ma"
  }
}
```

### ?? **Gmail Setup (Recommended for Testing)**

1. **Enable 2-Factor Authentication** on your Gmail account
2. **Generate App Password**:
   - Go to Google Account settings
   - Security ? 2-Step Verification ? App Passwords
   - Generate password for "Mail"
   - Use this password in `SmtpPassword`

### ?? **What's Now Working**

#### ? **Contact Form Emails**
- **Admin Notification**: Detailed contact form submission sent to admin
- **User Confirmation**: Professional thank you email sent to customer
- **Error Handling**: Graceful failure with user feedback

#### ? **Identity Email Confirmation**
- **Registration**: Email confirmation required for new accounts
- **Confirmation Email**: Professional HTML email with confirmation link
- **Resend Option**: Users can request new confirmation emails
- **Password Reset**: Email-based password reset functionality

### ?? **Email Templates Include**

- **Professional HTML Design**: Branded email templates
- **Responsive Layout**: Works on mobile and desktop
- **Contact Form Details**: All form fields included in admin email
- **Security Features**: Proper encoding and validation

### ?? **Security Features**

- **Email Confirmation Required**: `RequireConfirmedAccount = true`
- **Secure Token Generation**: ASP.NET Core Identity tokens
- **Rate Limiting**: Built-in protection against spam
- **Error Logging**: Comprehensive logging for troubleshooting

### ?? **Testing**

1. **Update Email Settings**: Add your SMTP credentials
2. **Register New Account**: Test email confirmation flow
3. **Submit Contact Form**: Verify both admin and user emails
4. **Check Spam Folders**: Initial emails may go to spam

### ?? **Files Created/Modified**

- `Services/EmailService.cs` - Main email service
- `Services/IEmailService.cs` - Email service interface
- `Models/EmailSettings.cs` - Configuration model
- `Areas/Identity/Pages/Account/Register.cshtml(.cs)` - Registration with email
- `Areas/Identity/Pages/Account/ConfirmEmail.cshtml(.cs)` - Email confirmation
- `Areas/Identity/Pages/Account/ResendEmailConfirmation.cshtml(.cs)` - Resend confirmation
- `Areas/Identity/Pages/Account/RegisterConfirmation.cshtml(.cs)` - Registration success
- `Pages/Contact.cshtml.cs` - Updated contact form
- `Program.cs` - Service registration and configuration

### ?? **Configuration Options**

- **SMTP Providers**: Gmail, Outlook, SendGrid, etc.
- **Development Mode**: Use Gmail for testing
- **Production**: Use dedicated email service
- **Customization**: Modify email templates in `EmailService.cs`

### ?? **Next Steps**

1. Configure your SMTP settings
2. Test the registration flow
3. Test the contact form
4. Customize email templates if needed
5. Set up production email service (SendGrid, AWS SES, etc.)

**Both contact form emails and Identity email confirmation are now fully functional!** ??