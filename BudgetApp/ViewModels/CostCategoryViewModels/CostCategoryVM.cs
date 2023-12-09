using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.CostCategoryViewModels
{
    public class CostCategoryVM
    {
        public Guid CostCategoryId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [Display(Name = "Budgeted Amount")]
        public double BudgetedAmount { get; set; }
    }
}
