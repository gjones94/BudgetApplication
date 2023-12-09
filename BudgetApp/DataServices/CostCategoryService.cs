using BudgetApp.Areas.Identity;
using BudgetApp.Data;
using BudgetApp.DataServices.BaseServices;
using BudgetApp.Models;

namespace BudgetApp.DataServices
{
    public class CostCategoryService : BaseArchivableEntityService<CostCategory>
    {
        private ILogger<CostCategoryService> _logger;

        public CostCategoryService(ApplicationDbContext dbContext, ICurrentUserAccessor currentUserAccessor, ILogger<CostCategoryService> logger) : base(dbContext, currentUserAccessor, logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtains the cost categories for a budget
        /// </summary>
        /// <param name="budgetId">Id of the budget being queried</param>
        /// <returns></returns>
        public IList<CostCategory> GetCurrentListForBudget(Guid budgetId)
        {
            IList<CostCategory> costCategories = GetAll().Where(costCategory => costCategory.Budget.EntityId == budgetId).ToList();
            return costCategories;
        }


        public double GetTotalCategoryAllocationForBudget(Guid budgetId)
        {
            return GetCurrentListForBudget(budgetId).Sum(c => c.BudgetedAmount);
        }


        public IList<string> GetAvailableCategories(Guid budgetId)
        {
            return GetCurrentListForBudget(budgetId).Select(c => c.Name).ToList();
        }
    }
}
