using BudgetApp.Areas.Identity;
using BudgetApp.Data;
using BudgetApp.DataServices.BaseServices;
using BudgetApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace BudgetApp.DataServices
{
    public class VariableCostService : BaseArchivableEntityService<VariableCost>
    {
        private ILogger<VariableCostService> _logger;
        private CostCategoryService _costCategoryService;

        public VariableCostService(ApplicationDbContext dbContext, CostCategoryService costCategoryService, ICurrentUserAccessor currentUserAccessor, ILogger<VariableCostService> logger) : base(dbContext, currentUserAccessor, logger)
        {
            _logger = logger;
            _costCategoryService = costCategoryService;
        }

        /// <summary>
        /// Returns the current period list of variable costs for a particular budget
        /// </summary>
        /// <param name="budgetId"></param>
        /// <returns></returns>
        public IList<VariableCost> GetCurrentListForBudget(Guid budgetId)
        {
            DateTime MonthBegin = DateTime.Now.GetMonthBegin();
            DateTime MonthEnd = DateTime.Now.GetMonthEnd();

            IList<VariableCost> variableCosts = GetAll().Include(variableCost => variableCost.Category).Where(variableCost => variableCost.Budget.EntityId == budgetId && variableCost.DateIncurred >= MonthBegin && variableCost.DateIncurred <= MonthEnd).OrderByDescending(v => v.DateIncurred).ToList();
            return variableCosts;
        }

        /// <summary>
        /// Obtains the current period list of variable costs for a particular category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IList<VariableCost> GetCurrentListForCategory(Guid categoryId)
        { 
            DateTime MonthBegin = DateTime.Now.GetMonthBegin();
            DateTime MonthEnd = DateTime.Now.GetMonthEnd();

            return GetAll().Where(v => v.Category.EntityId == categoryId && v.DateIncurred <= MonthEnd && v.DateIncurred >= MonthBegin).ToList();
        }


        /// <summary>
        /// Obtains the current period total of the variable costs for a particular budget
        /// </summary>
        /// <param name="budgetId"></param>
        /// <returns><see cref="double"/> Total Amount</returns>
        public double GetCurrentTotalForBudget(Guid budgetId)
        {
            return GetCurrentListForBudget(budgetId).Sum(f => f.Amount);
        }

        /// <summary>
        /// Obtains the current period total of the variable costs for a particular category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns><see cref="double"/> Total Amount</returns>
        public double GetCurrentVariableCostForCategory(Guid categoryId)
        {
            return GetCurrentListForCategory(categoryId).Sum(v => v.Amount);
        }

        /// <summary>
        /// Obtains current period totals for each transaction category of a budget
        /// </summary>
        /// <param name="budgetId"></param>
        /// <returns>Current Totals Per Category</returns>
        public Dictionary<CostCategory, double> GetCurrentCategoryTotals(Guid budgetId) 
        { 
            Dictionary<CostCategory, double> categoryTotals = new Dictionary<CostCategory, double>();
            IList<CostCategory> categories = _costCategoryService.GetCurrentListForBudget(budgetId);

            foreach(CostCategory category in categories)
            {
                double total = GetCurrentVariableCostForCategory(category.EntityId);
                categoryTotals[category] = total;
            }

            return categoryTotals;
        }
    }
}
