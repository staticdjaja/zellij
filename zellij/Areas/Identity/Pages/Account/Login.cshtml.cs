using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace zellij.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = default!;

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Email or Username")]
            public string Email { get; set; } = default!;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = default!;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Try to find user by email or username
                var user = await _userManager.FindByEmailAsync(Input.Email) ?? await _userManager.FindByNameAsync(Input.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "No account found with that email or username. Please check your credentials or register for a new account.");
                    return Page();
                }

                // Check if user is locked out
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var timeRemaining = lockoutEnd?.Subtract(DateTimeOffset.Now).TotalMinutes ?? 0;
                    
                    ModelState.AddModelError(string.Empty, 
                        $"Your account has been locked due to multiple failed login attempts. Please try again in {Math.Ceiling(timeRemaining)} minutes.");
                    _logger.LogWarning("User {Email} attempted to login while locked out.", Input.Email);
                    return Page();
                }

                // Check if email is confirmed (if required by your app settings)
                if (!user.EmailConfirmed && _signInManager.Options.SignIn.RequireConfirmedAccount)
                {
                    ModelState.AddModelError(string.Empty, 
                        "Please confirm your email address before signing in. Check your email for a confirmation link.");
                    return Page();
                }

                // Attempt to sign in
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, Input.Password, Input.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully.", Input.Email);
                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var timeRemaining = lockoutEnd?.Subtract(DateTimeOffset.Now).TotalMinutes ?? 0;
                    
                    ModelState.AddModelError(string.Empty, 
                        $"Your account has been locked due to multiple failed login attempts. Please try again in {Math.Ceiling(timeRemaining)} minutes or contact support.");
                    _logger.LogWarning("User {Email} account locked out.", Input.Email);
                    return Page();
                }

                if (result.IsNotAllowed)
                {
                    if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, 
                            "Please confirm your email address before signing in. Check your email for a confirmation link or request a new one.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, 
                            "Your account is not allowed to sign in. Please contact support for assistance.");
                    }
                    return Page();
                }

                // Password is incorrect
                var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                var attemptsRemaining = maxAttempts - failedAttempts;

                if (attemptsRemaining > 1)
                {
                    ModelState.AddModelError(string.Empty, 
                        $"Incorrect password. You have {attemptsRemaining} attempts remaining before your account is temporarily locked.");
                }
                else if (attemptsRemaining == 1)
                {
                    ModelState.AddModelError(string.Empty, 
                        "Incorrect password. Warning: This is your last attempt before your account is temporarily locked for security.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, 
                        "Incorrect password. Your account will be temporarily locked for security.");
                }

                _logger.LogWarning("Failed login attempt for user {Email}. Attempts remaining: {AttemptsRemaining}", 
                    Input.Email, attemptsRemaining);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}