using BudgetApp.ViewModels.CostCategoryViewModels;

namespace BudgetApp.ViewModels.DashboardViewModels
{
    public class DashboardVM
    {
        public DashboardVM()
        {
            CategoryBTAList = new List<BudgetToActualVM>();
            CategoryNames = new List<string>();
        }

        public double Income { get; set; }

        public double FixedCosts { get; set; }

        public double VariableCosts { get; set; }

        public double TotalExpenses => FixedCosts + VariableCosts;

        public double SavingsGoal { get; set; }

        public double SavingsActual { get; set; }

        public IList<BudgetToActualVM> CategoryBTAList { get; set; }

        public List<string> CategoryNames { get; set; }
    }
}
