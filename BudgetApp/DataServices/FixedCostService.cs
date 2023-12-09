using BudgetApp.Areas.Identity;
using BudgetApp.Data;
using BudgetApp.DataServices.BaseServices;
using BudgetApp.Models;

namespace BudgetApp.DataServices
{
    public class FixedCostService : BaseArchivableEntityService<FixedCost>
    {
        private ILogger<FixedCostService> _logger;

        public FixedCostService(ApplicationDbContext dbContext, ICurrentUserAccessor currentUserAccessor, ILogger<FixedCostService> logger) : base(dbContext, currentUserAccessor, logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtains the current period fixed costs for a budget
        /// </summary>
        /// <param name="budgetId">Id of the budget being queried</param>
        /// <returns></returns>
        public IList<FixedCost> GetCurrentListForBudget(Guid budgetId)
        {
            DateTime DateNow = DateTime.Now.DateOnly();
            IList<FixedCost> fixedCosts = GetAll().Where(fixedCost => fixedCost.Budget.EntityId == budgetId && fixedCost.MonthBegin <= DateNow && fixedCost.MonthEnd >= DateNow).ToList();
            return fixedCosts;
        }

        public double GetCurrentTotalForBudget(Guid budgetId)
        {
            return GetCurrentListForBudget(budgetId).Sum(f => f.Amount);
        }
    }
}
