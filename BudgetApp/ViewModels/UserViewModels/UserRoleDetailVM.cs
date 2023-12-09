using BudgetApp.Areas.Identity.Models;

namespace BudgetApp.ViewModels.UserViewModels
{
    public class UserRoleDetailVM
    {
        public Guid UserId { get; set; }

        public string? FirstName { get; set; }  

        public string? LastName { get; set; }

        public string? Role { get; set; }
    }
}
