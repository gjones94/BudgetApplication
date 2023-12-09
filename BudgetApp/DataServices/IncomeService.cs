using BudgetApp.Areas.Identity;
using BudgetApp.Data;
using BudgetApp.DataServices.BaseServices;
using BudgetApp.Models;

namespace BudgetApp.DataServices
{
    public class IncomeService : BaseArchivableEntityService<Income>
    {
        private ILogger<IncomeService> _logger;

        public IncomeService(ApplicationDbContext dbContext, ICurrentUserAccessor currentUserAccessor, ILogger<IncomeService> logger) : base(dbContext, currentUserAccessor, logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtains the current period incomes for a budget
        /// </summary>
        /// <param name="budgetId">Id of the budget being queried</param>
        /// <returns></returns>
        public IList<Income> GetCurrentListForBudget(Guid budgetId)
        {
            DateTime DateNow = DateTime.Now.DateOnly();
            IList<Income> incomes = GetAll().Where(income => income.Budget.EntityId == budgetId && income.MonthBegin <= DateNow && income.MonthEnd >= DateNow).ToList();
            return incomes;
        }

        public double GetCurrentTotalForBudget(Guid budgetId)
        {
            return GetCurrentListForBudget(budgetId).Sum(i => i.Amount);
        }
    }
}
