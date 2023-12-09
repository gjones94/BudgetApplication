using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.UserViewModels
{
    public class UserRoleEditVM
    {
        public UserRoleEditVM() 
        {
            Name = string.Empty;
            SelectedRole = string.Empty;
            AvailableRoles = new List<string>();
        } 

        public string Name { get; set; }

        public Guid UserId { get; set; }

        public Guid EntityId { get; set; }

        public string SelectedRole { get; set; }

        [Display(Name = "Role")]
        public List<string> AvailableRoles { get; set; }

        public string ReturnUrl { get; set; }
    }
}
