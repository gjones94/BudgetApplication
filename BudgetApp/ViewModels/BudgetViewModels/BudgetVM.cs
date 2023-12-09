using System.ComponentModel.DataAnnotations;

namespace BudgetApp.ViewModels.BudgetViewModels
{
    public class BudgetVM
    {

        public BudgetVM() { }

        public BudgetVM(Guid budgetId, double monthlySavingsGoal, string returnUrl) {
            BudgetId = budgetId;
            MonthlySavingsGoal = monthlySavingsGoal;
            ReturnUrl = returnUrl;
        }

        public Guid BudgetId { get; set; }

        [Display(Name = "Monthly Savings Goal")]
        public double MonthlySavingsGoal { get; set; }

        public string ReturnUrl { get; set; }
    }
}
