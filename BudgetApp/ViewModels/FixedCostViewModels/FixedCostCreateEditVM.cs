using BudgetApp.Areas.Identity.Models;
using BudgetApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.FixedCostViewModels
{
    public class FixedCostCreateEditVM
    {
        public FixedCostCreateEditVM()
        {
            ReturnUrl = string.Empty;
            FixedCostVM = new FixedCostVM();
        }

        public Guid ParentId { get; set; }

        public FixedCostVM FixedCostVM { get; set; }

        public string ReturnUrl { get; set; }
    }
}
