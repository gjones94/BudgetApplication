using BudgetApp.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Areas.Identity.Models
{
    public class User : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime? BirthDate { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public bool ApprovedByAdmin { get; set; } = false;
    }
}
