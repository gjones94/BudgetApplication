using Microsoft.AspNetCore.Identity;

namespace BudgetApp.Areas.Identity.Models
{
    public class Role : IdentityRole<Guid>
    {
        public string? Description { get; set; }

        public Role() : base() { }

    }
}
