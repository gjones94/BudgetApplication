using BudgetApp.Areas.Identity;
using BudgetApp.Areas.Identity.Models;
using BudgetApp.DataServices;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.EmailServices;
using BudgetApp.Models;
using BudgetApp.ViewModels.BudgetViewModels;
using BudgetApp.ViewModels.CostCategoryViewModels;
using BudgetApp.ViewModels.ErrorViewModels;
using BudgetApp.ViewModels.FixedCostViewModels;
using BudgetApp.ViewModels.IncomeViewModels;
using BudgetApp.ViewModels.UserViewModels;
using BudgetApp.ViewModels.VariableCostViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using static BudgetApp.Errors;

namespace BudgetApp.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        ILogger _logger;
        EntityServiceManager _entityServiceManager;
        UserServiceManager _userServiceManager;

        public BudgetController(ILogger<BudgetController> logger, EntityServiceManager entityServiceManager, UserServiceManager userServiceManager)
        {
            _logger = logger;
            _entityServiceManager = entityServiceManager;
            _userServiceManager = userServiceManager;
        }


        [HttpGet]
        public IActionResult Create()
        {
            User? user = _userServiceManager.GetCurrentUser();

            if (user == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthenticated));
            }

            Budget budget = new Budget() { MonthlySavingsGoal = 0 };

            bool success = _entityServiceManager.BudgetService.Add(budget);

            if (success)
            {
                success = _userServiceManager.AddUserToEntity(user.Id, budget.EntityId, nameof(Budget), RoleName.Owner);
            }

            if (success)
            {
                _entityServiceManager.BudgetService.SaveChanges();
                return RedirectToAction("Details", new { Id = budget.EntityId });
            }
            else
            {
                return View("Error", GetErrorViewModel(ErrorType.OperationFailed, message: "Failed to create budget"));
            }
        }

        [HttpPost]
        public IActionResult Edit(BudgetVM budgetVM)
        {
            Budget? budget = _entityServiceManager.BudgetService.GetById(budgetVM.BudgetId);
            if(budget != null)
            {
                budget.MonthlySavingsGoal = budgetVM.MonthlySavingsGoal;

                bool success = _entityServiceManager.BudgetService.Update(budget);

                if (success)
                {
                    _entityServiceManager.BudgetService.SaveChanges();
                    return Redirect(budgetVM.ReturnUrl);
                }
            }

            return View("Error", GetErrorViewModel(ErrorType.OperationFailed, message: "Failed to modify budget"));
        }

        public IActionResult Details()
        {
            User? user = _userServiceManager.GetCurrentUser();
            string returnUrl = string.Empty;

            if (user == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthenticated));
            }

            Budget? budget = _userServiceManager.GetEntityForUser<Budget>(user.Id);

            if (budget != null)
            {
                returnUrl = Url.Action("Details", "Budget");

                BudgetDetailVM viewModel = new BudgetDetailVM(budget.EntityId, budget.MonthlySavingsGoal, returnUrl);

                //Retrieve all users for this entity
                IList<UserToEntity> usersToEntities = _userServiceManager.GetAllRelationshipsForEntity(budget.EntityId);

                //obtain user detailed information
                foreach (UserToEntity userToEntity in usersToEntities)
                {
                    User? budgetUser = _userServiceManager.GetUserById(userToEntity.UserId);
                    if (budgetUser == null)
                    {
                        _logger.LogError($"Error retrieving User with Id: {userToEntity.UserId}");
                    }
                    else
                    {
                        viewModel.UserRoles.Add(new UserRoleDetailVM() { UserId = budgetUser.Id, FirstName = budgetUser.FirstName, LastName = budgetUser.LastName, Role = userToEntity.Role });
                    }
                }

                IList<Income> incomes = _entityServiceManager.IncomeService.GetCurrentListForBudget(budget.EntityId);

                foreach (Income income in incomes)
                {
                    IncomeVM incomeVM = new IncomeVM()
                    {
                        IncomeId = income.EntityId,
                        Amount = income.Amount,
                        Description = income.Description,
                        NameOfUser = _userServiceManager.GetUserFullName(income.UserId)
                    };
                    viewModel.Incomes.Add(incomeVM);
                }

                IList<FixedCost> fixedCosts = _entityServiceManager.FixedCostService.GetCurrentListForBudget(budget.EntityId);

                foreach (FixedCost fixedCost in fixedCosts)
                {
                    FixedCostVM fixedCostVM = new FixedCostVM()
                    {
                        FixedCostId = fixedCost.EntityId,
                        Amount = fixedCost.Amount,
                        Description = fixedCost.Description
                    };

                    viewModel.FixedCosts.Add(fixedCostVM);
                }

                IList<VariableCost> variableCosts = _entityServiceManager.VariableCostService.GetCurrentListForBudget(budget.EntityId);

                foreach (VariableCost variableCost in variableCosts)
                {
                    VariableCostVM variableCostVM = new VariableCostVM()
                    {
                        VariableCostId = variableCost.EntityId,
                        Amount = variableCost.Amount,
                        Description = variableCost.Description,
                        NameOfUser = _userServiceManager.GetUserFullName(variableCost.UserId),
                        CategoryName = variableCost.Category.Name,
                        DateIncurred = variableCost.DateIncurred
                    };
                    viewModel.VariableCosts.Add(variableCostVM);
                }

                IList<CostCategory> costCategories = _entityServiceManager.CostCategoryService.GetCurrentListForBudget(budget.EntityId);
                foreach (CostCategory costCategory in costCategories)
                {
                    CostCategoryVM costCategoryVM = new CostCategoryVM()
                    {
                        CostCategoryId = costCategory.EntityId,
                        BudgetedAmount = costCategory.BudgetedAmount,
                        Name = costCategory.Name
                    };
                    viewModel.CostCategories.Add(costCategoryVM);
                }

                viewModel.RemainingCategoryAllocation = _entityServiceManager.BudgetService.GetRemainingCategoryAllocationForBudget(budget.EntityId);

                //Check permissions of current user
                ViewBag.UserIsOwner = _userServiceManager.GetUserRoleForEntity(user.Id, budget.EntityId) == RoleName.Owner;
                ViewBag.UserCanEdit = _userServiceManager.GetUserRoleForEntity(user.Id, budget.EntityId) == RoleName.User;
                return View(viewModel);
        }

            return View("Error", GetErrorViewModel(ErrorType.NotFound, title: $"{nameof(Budget)} Not Found"));
        }

        /// <summary>
        /// Allows user to remove themselves from the budget
        /// </summary>
        /// <param name="budgetId"></param>
        /// <returns></returns>
        public IActionResult Leave(Guid budgetId)
        {
            User? currentUser = _userServiceManager.GetCurrentUser();

            if (currentUser == null)
            {
                return Json( new { title = Errors.Unauthenticated.Title, message =  Errors.Unauthenticated.Message });
            }

            bool success = _userServiceManager.RemoveFromEntitySafely(currentUser.Id, budgetId, RoleName.Owner);

            if(success)
            {
                return Json( new { status = "success", title = "Success", message = "You have left the budget", returnUrl = Url.Action("Index", "Home") });
            }
            else
            {
                return Json(new { title = Errors.OperationFailed.Title, message = "Failed to leave budget" });
            }
        }

        /// <summary>
        /// Deletes a budget
        /// </summary>
        /// <param name="budgetId"></param>
        /// <returns></returns>
        public IActionResult Remove(Guid budgetId)
        {
            User? currentUser = _userServiceManager.GetCurrentUser();

            //check if user is authenticated and found
            if(currentUser == null)
            {
                return Json( new { title = Errors.Unauthenticated.Title, message =  Errors.Unauthenticated.Message });
            }

            //check if user is authorized
            if (_userServiceManager.UserIsAuthorizedForAction(currentUser.Id, budgetId, UserAction.Delete) == false)
            {
                return Json( new { title = Errors.Unauthorized.Title, message =  Errors.Unauthorized.Message });
            }

            //if more than one owner exists, you cannot delete budget
            var ownersOnBudget = _userServiceManager.GetAllRelationshipsForEntity(budgetId).Where(row => row.Role == RoleName.Owner).ToList();

            if (ownersOnBudget.Count > 1)
            {
                return Json(new { title = InvalidOperation.Title, message = "There is more than one owner associated with this budget. You may leave the budget, but you cannot delete it" });
            }

            Budget? budget = _entityServiceManager.BudgetService.GetById(budgetId);

            if(budget == null)
            {
                return Json(new { title = Errors.NotFound.Title, message = Errors.NotFound.Message });
            }

            _entityServiceManager.BudgetService.Remove(budget);
            _userServiceManager.RemoveAllUsersForEntity(budget.EntityId);
            _entityServiceManager.SaveChanges();

            return Json(new { status = "success", title = "Success", message = "Budget has been deleted", returnUrl = Url.Action("Index", "Home") });
        }
    }
}
