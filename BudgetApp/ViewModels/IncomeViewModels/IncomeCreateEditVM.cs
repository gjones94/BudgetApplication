using BudgetApp.Areas.Identity.Models;
using BudgetApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.IncomeViewModels
{
    public class IncomeCreateEditVM
    {
        public IncomeCreateEditVM()
        {
            AvailableUsers = new List<User>();
            ReturnUrl = string.Empty;
            IncomeVM = new IncomeVM();
        }

        public Guid ParentId { get; set; }

        public IncomeVM IncomeVM { get; set; }

        public Guid UserId { get; set; }

        [Display(Name = "User")]
        public IList<User> AvailableUsers { get; set; }

        public string ReturnUrl { get; set; }
    }
}
