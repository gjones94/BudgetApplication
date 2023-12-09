using Org.BouncyCastle.Bcpg;
using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;

namespace BudgetApp.ViewModels.CostCategoryViewModels
{
    public class BudgetToActualVM
    {
        [Required]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [Display(Name = "Budgeted Amount")]
        public double BudgetedAmount { get; set; }

        public bool OverBudget => SpentAmount > BudgetedAmount;

        public double SpentAmount { get; set; }

        public double SpentPercent => (SpentAmount / BudgetedAmount) * 100;

        public double DeficitAmount => (SpentAmount - BudgetedAmount);

        public double DeficitPercent => (DeficitAmount / SpentAmount) * 100;
    }
}
