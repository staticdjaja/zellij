# Two-Factor Authentication with QR Codes

This project now includes full support for Two-Factor Authentication (2FA) with QR code generation for the CESARO website.

## Features Implemented

### ? QR Code Generation
- **QRCoder Library**: Added QRCoder package for generating QR codes
- **QR Service**: Custom service to generate QR codes as base64 data URIs
- **Visual QR Codes**: QR codes displayed directly in the web interface

### ? Two-Factor Authentication Pages
- **Enable Authenticator**: `/Identity/Account/Manage/EnableAuthenticator`
- **Two-Factor Management**: `/Identity/Account/Manage/TwoFactorAuthentication`
- **Recovery Codes**: `/Identity/Account/Manage/ShowRecoveryCodes`
- **Generate New Codes**: `/Identity/Account/Manage/GenerateRecoveryCodes`
- **Reset Authenticator**: `/Identity/Account/Manage/ResetAuthenticator`
- **Disable 2FA**: `/Identity/Account/Manage/Disable2fa`

### ? User Interface Enhancements
- **Styled QR Codes**: Professional-looking QR code display with instructions
- **Step-by-Step Guide**: Clear instructions for setting up authenticator apps
- **Recovery Code Management**: Print and copy functionality for recovery codes
- **Responsive Design**: Mobile-friendly 2FA setup pages
- **User Navigation**: Easy access to 2FA settings through user dropdown

## How to Use

### For Users:
1. **Login** to your account
2. **Click your username** in the top navigation
3. **Select "Two-Factor Auth"** from the dropdown
4. **Click "Add authenticator app"**
5. **Scan the QR code** with your authenticator app (Microsoft Authenticator, Google Authenticator, etc.)
6. **Enter the verification code** from your app
7. **Save your recovery codes** in a safe place

### For Administrators:
- Users with 2FA enabled will show a security badge in the admin user management section
- Recovery codes are automatically generated when 2FA is first enabled
- Users can generate new recovery codes if needed

## Supported Authenticator Apps

- **Microsoft Authenticator** (Android/iOS)
- **Google Authenticator** (Android/iOS)
- **Authy** (Cross-platform)
- **1Password** (Built-in authenticator)
- **Any TOTP-compatible app**

## Technical Details

### Dependencies Added:
```xml
<PackageReference Include="QRCoder" Version="1.4.3" />
```

### Services Registered:
```csharp
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
```

### Identity Configuration:
```csharp
options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
```

## Security Features

- **TOTP (Time-based One-Time Password)** standard
- **Recovery codes** for account recovery
- **Secure key generation** using ASP.NET Core Identity
- **QR codes** contain properly formatted otpauth:// URIs
- **Backup options** for users who lose their devices

## File Structure

```
Areas/Identity/Pages/Account/Manage/
??? EnableAuthenticator.cshtml(.cs)
??? TwoFactorAuthentication.cshtml(.cs)
??? ShowRecoveryCodes.cshtml(.cs)
??? GenerateRecoveryCodes.cshtml(.cs)
??? ResetAuthenticator.cshtml(.cs)
??? Disable2fa.cshtml(.cs)
??? ManageNavPages.cs
??? _StatusMessage.cshtml

Services/
??? QrCodeService.cs
```

## Usage Instructions

The 2FA functionality is now fully integrated into the existing user management system. Users can access it through:

1. **Main Navigation**: User dropdown ? "Two-Factor Auth"
2. **Profile Settings**: When managing their account
3. **Admin View**: Administrators can see which users have 2FA enabled

All QR codes are generated server-side and displayed as images, making the setup process seamless for users across all devices and browsers.