using BudgetApp.Areas.Identity.Models;
using BudgetApp.Data;
using BudgetApp.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using BudgetApp.Models.Interfaces;

namespace BudgetApp.DataServices.ServiceManagers
{
    public class EntityServiceManager
    {
        private BudgetService _budgetService;
        private ApplicationDbContext _dbContext;
        private IncomeService _incomeService;
        private FixedCostService _fixedCostService;
        private VariableCostService _variableCostService;
        private CostCategoryService _costCategoryService;

        public EntityServiceManager(ApplicationDbContext dbContext, BudgetService budgetService, IncomeService incomeService, FixedCostService fixedCostService, VariableCostService variableCostService, CostCategoryService costCategoryService)
        {
            _budgetService = budgetService;
            _dbContext = dbContext;
            _incomeService = incomeService;
            _fixedCostService = fixedCostService;
            _variableCostService = variableCostService;
            _costCategoryService = costCategoryService;
        }

        public BudgetService BudgetService => _budgetService;

        public IncomeService IncomeService => _incomeService;

        public FixedCostService FixedCostService => _fixedCostService;

        public VariableCostService VariableCostService => _variableCostService;

        public CostCategoryService CostCategoryService => _costCategoryService;

        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }
    }
}
