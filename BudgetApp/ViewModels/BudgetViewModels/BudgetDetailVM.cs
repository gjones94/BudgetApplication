using BudgetApp.Areas.Identity.Models;
using BudgetApp.Models;
using BudgetApp.ViewModels.CostCategoryViewModels;
using BudgetApp.ViewModels.FixedCostViewModels;
using BudgetApp.ViewModels.IncomeViewModels;
using BudgetApp.ViewModels.UserViewModels;
using BudgetApp.ViewModels.VariableCostViewModels;

namespace BudgetApp.ViewModels.BudgetViewModels
{
    public class BudgetDetailVM
    {
        public BudgetDetailVM() { }

        public BudgetDetailVM(Guid budgetId, double monthlySavingsGoal, string returnUrl)
        {
            BudgetVM = new BudgetVM(budgetId, monthlySavingsGoal, returnUrl);
            UserRoles = new List<UserRoleDetailVM>();
            Incomes = new List<IncomeVM>();
            FixedCosts = new List<FixedCostVM>();
            VariableCosts = new List<VariableCostVM>();
            CostCategories = new List<CostCategoryVM>();
        }

        public BudgetVM BudgetVM { get; set; }

        public double RemainingCategoryAllocation { get; set; }

        public IList<UserRoleDetailVM> UserRoles { get; set; }

        public IList<IncomeVM> Incomes { get; set; }

        public IList<FixedCostVM> FixedCosts { get; set; }

        public IList<VariableCostVM> VariableCosts { get; set; }

        public IList<CostCategoryVM> CostCategories { get; set; }
    }
}
