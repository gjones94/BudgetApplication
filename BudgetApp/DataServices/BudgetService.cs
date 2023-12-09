using BudgetApp.Areas.Identity;
using BudgetApp.Data;
using BudgetApp.DataServices.BaseServices;
using BudgetApp.Models;
using Microsoft.Identity.Client;

namespace BudgetApp.DataServices
{

    public class BudgetService : BaseArchivableEntityService<Budget>
    {
        private ILogger<BudgetService> _logger;
        CostCategoryService _costCategoryService;
        FixedCostService _fixedCostService;
        IncomeService _incomeService;
        VariableCostService _variableCostService;

        public BudgetService(ApplicationDbContext dbContext, CostCategoryService categoryService, IncomeService incomeService, FixedCostService fixedCostService, VariableCostService variableCostService, ICurrentUserAccessor currentUserAccessor, ILogger<BudgetService> logger) : base(dbContext, currentUserAccessor, logger) 
        {
            _logger = logger;
            _costCategoryService = categoryService;
            _fixedCostService = fixedCostService;
            _incomeService = incomeService;
            _variableCostService = variableCostService;
        }

        public double GetRemainingCategoryAllocationForBudget(Guid budgetId)
        {
            Budget? budget = GetById(budgetId);
            if(budget == null)
            {
                return 0;
            }

            double TotalIncome = _incomeService.GetCurrentTotalForBudget(budgetId);
            double SavingsGoal = budget.MonthlySavingsGoal;
            double TotalFixedCosts = _fixedCostService.GetCurrentTotalForBudget(budgetId);
            double CurrentCategoryAllocatedAmount = _costCategoryService.GetTotalCategoryAllocationForBudget(budgetId);

            return TotalIncome - TotalFixedCosts - SavingsGoal - CurrentCategoryAllocatedAmount;
        }

        public double GetCurrentSavings(Guid budgetId)
        {
            double TotalIncome = _incomeService.GetCurrentTotalForBudget(budgetId);
            double TotalVariableCosts = _variableCostService.GetCurrentTotalForBudget(budgetId);
            double TotalFixedCosts = _fixedCostService.GetCurrentTotalForBudget(budgetId);

            return TotalIncome - TotalFixedCosts - TotalVariableCosts;
        }
    }
}