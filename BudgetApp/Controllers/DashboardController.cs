using BudgetApp.Areas.Identity.Models;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.Models;
using BudgetApp.ViewModels.CostCategoryViewModels;
using BudgetApp.ViewModels.DashboardViewModels;
using Microsoft.AspNetCore.Mvc;
using static BudgetApp.Errors;

namespace BudgetApp.Controllers
{
    public class DashboardController : Controller
    {
        ILogger _logger;
        EntityServiceManager _entityServiceManager;
        UserServiceManager _userServiceManager;

        public DashboardController(ILogger<DashboardController> logger, EntityServiceManager entityServiceManager, UserServiceManager userServiceManager)
        {
            _logger = logger;
            _entityServiceManager = entityServiceManager;
            _userServiceManager = userServiceManager;
        }

        public IActionResult Index()
        {
            User? user = _userServiceManager.GetCurrentUser();

            if (user == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthenticated));
            }

            Budget? budget = _userServiceManager.GetEntityForUser<Budget>(user.Id);

            if (budget != null)
            {
                DashboardVM viewModel = new DashboardVM()
                {
                    Income = _entityServiceManager.IncomeService.GetCurrentTotalForBudget(budget.EntityId),
                    VariableCosts = _entityServiceManager.VariableCostService.GetCurrentTotalForBudget(budget.EntityId),
                    FixedCosts = _entityServiceManager.FixedCostService.GetCurrentTotalForBudget(budget.EntityId),
                    SavingsGoal = budget.MonthlySavingsGoal,
                    SavingsActual = _entityServiceManager.BudgetService.GetCurrentSavings(budget.EntityId)
                };


                var costCategoryActuals = _entityServiceManager.VariableCostService.GetCurrentCategoryTotals(budget.EntityId);
                foreach( var costCategoryActual in costCategoryActuals)
                {
                    BudgetToActualVM btaVM = new BudgetToActualVM()
                    {
                        Name = costCategoryActual.Key.Name,
                        BudgetedAmount = costCategoryActual.Key.BudgetedAmount,
                        SpentAmount = costCategoryActual.Value,
                    };

                    viewModel.CategoryBTAList.Add(btaVM);
                    viewModel.CategoryNames = viewModel.CategoryBTAList.Select(c => c.Name).ToList();
                }

                return View(viewModel);
            }

            return View("Error", GetErrorViewModel(ErrorType.NotFound));
        }
    }
}
