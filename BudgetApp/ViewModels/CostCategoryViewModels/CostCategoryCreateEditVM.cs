using BudgetApp.Areas.Identity.Models;
using BudgetApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.CostCategoryViewModels
{
    public class CostCategoryCreateEditVM
    {
        public CostCategoryCreateEditVM()
        {
            ReturnUrl = string.Empty;
            CostCategoryVM = new CostCategoryVM();
        }

        public Guid ParentId { get; set; }

        public CostCategoryVM CostCategoryVM { get; set; }

        public string ReturnUrl { get; set; }
    }
}
