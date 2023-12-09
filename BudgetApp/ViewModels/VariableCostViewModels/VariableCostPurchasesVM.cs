using BudgetApp.Models;

namespace BudgetApp.ViewModels.VariableCostViewModels
{
    public class VariableCostPurchasesVM
    {
        public IList<VariableCostVM> VariableCosts { get; set; } 

        public IList<string> AvailableCategories { get; set; }

        public string ReturnUrl { get; set; }
       
    }
}
