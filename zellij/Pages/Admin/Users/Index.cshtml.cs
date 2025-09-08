using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace zellij.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        public readonly UserManager<IdentityUser> UserManager;

        public IndexModel(UserManager<IdentityUser> userManager)
        {
            UserManager = userManager;
        }

        public IList<IdentityUser> Users { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task OnGetAsync()
        {
            IQueryable<IdentityUser> usersQuery = UserManager.Users;

            if (!string.IsNullOrEmpty(SearchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.UserName!.Contains(SearchString) ||
                    u.Email!.Contains(SearchString) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(SearchString)));
            }

            Users = await usersQuery
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }
    }
}