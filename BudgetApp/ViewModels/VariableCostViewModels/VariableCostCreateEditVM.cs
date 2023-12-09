using BudgetApp.Areas.Identity.Models;
using BudgetApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.VariableCostViewModels
{
    public class VariableCostCreateEditVM
    {
        public VariableCostCreateEditVM()
        {
            AvailableUsers = new List<User>();
            AvailableCategories = new List<CostCategory>();
            ReturnUrl = string.Empty;
            VariableCostVM = new VariableCostVM();
        }

        public Guid ParentId { get; set; }

        public VariableCostVM VariableCostVM { get; set; }

        [Display(Name = "User")]
        public IList<User> AvailableUsers { get; set; }

        public Guid UserId { get; set; }

        [Display(Name = "Category")]
        public IList<CostCategory> AvailableCategories { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }

        public string ReturnUrl { get; set; }
    }
}
