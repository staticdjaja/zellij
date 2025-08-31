using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using zellij.Services;

namespace zellij.Pages
{
    public class ContactModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactModel> _logger;

        public ContactModel(IEmailService emailService, ILogger<ContactModel> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public ContactForm Contact { get; set; } = new();

        public string? ProductName { get; set; }

        public void OnGet(string? product)
        {
            ProductName = product;
            if (!string.IsNullOrEmpty(product))
            {
                Contact.Subject = $"Inquiry about {product}";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _emailService.SendContactFormEmailAsync(
                    Contact.Name,
                    Contact.Email,
                    Contact.Subject,
                    Contact.Message,
                    Contact.Company,
                    Contact.Phone,
                    Contact.ProjectType,
                    Contact.ProjectSize
                );

                TempData["SuccessMessage"] = "Thank you for your message! We have sent you a confirmation email and will contact you within 24 hours.";
                _logger.LogInformation("Contact form submitted successfully by {Name} ({Email})", Contact.Name, Contact.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact form email from {Email}", Contact.Email);
                TempData["ErrorMessage"] = "There was an error sending your message. Please try again or contact us directly.";
            }

            return RedirectToPage("./Contact");
        }
    }

    public class ContactForm
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string? Company { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Project Type")]
        public string? ProjectType { get; set; }

        [Display(Name = "Estimated Project Size (sq ft)")]
        public int? ProjectSize { get; set; }
    }
}