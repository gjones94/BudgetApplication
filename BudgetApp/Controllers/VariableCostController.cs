using BudgetApp.Areas.Identity.Models;
using BudgetApp.DataServices;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.Models;
using BudgetApp.ViewModels.BudgetViewModels;
using BudgetApp.ViewModels.IncomeViewModels;
using BudgetApp.ViewModels.VariableCostViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using static BudgetApp.Errors;

namespace BudgetApp.Controllers
{
    public class VariableCostController : Controller
    {
        ILogger _logger;
        EntityServiceManager _entityServiceManager;
        UserServiceManager _userServiceManager;

        public VariableCostController(ILogger<VariableCostController> logger, EntityServiceManager entityServiceManager, UserServiceManager userServiceManager) 
        {
            _logger = logger;
            _entityServiceManager = entityServiceManager;
            _userServiceManager = userServiceManager;
        }

        public IActionResult Index()
        {
            User? currentUser = _userServiceManager.GetCurrentUser();
            if(currentUser == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthenticated));
            }

            Budget? budget = _userServiceManager.GetEntityForUser<Budget>(currentUser.Id);
            if(budget == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            IList<VariableCost> variableCosts = _entityServiceManager.VariableCostService.GetCurrentListForBudget(budget.EntityId);

            IList<VariableCostVM> variableCostDetails = new List<VariableCostVM>();

            foreach(VariableCost variableCost in variableCosts)
            {
                VariableCostVM variableCostVM = new VariableCostVM()
                {
                    VariableCostId = variableCost.EntityId,
                    Amount = variableCost.Amount,
                    Description = variableCost.Description,
                    DateIncurred = variableCost.DateIncurred,
                    NameOfUser = _userServiceManager.GetUserFullName(variableCost.UserId),
                    CategoryName = variableCost.Category.Name
                };
                variableCostDetails.Add(variableCostVM);
            }

            VariableCostPurchasesVM viewModel = new VariableCostPurchasesVM()
            {
                VariableCosts = variableCostDetails,
                AvailableCategories = _entityServiceManager.CostCategoryService.GetAvailableCategories(budget.EntityId),
                ReturnUrl = Url.Action("Index")
            };

            return View(viewModel);
        }

        public IActionResult Create(Guid budgetId, string returnUrl)
        {
            Budget? budget = _entityServiceManager.BudgetService.GetById(budgetId);

            if(budget == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            VariableCostCreateEditVM viewModel = new VariableCostCreateEditVM()
            {
                VariableCostVM = new VariableCostVM(),
                AvailableUsers = _userServiceManager.GetAllUsersForEntity(budget.EntityId),
                AvailableCategories = _entityServiceManager.CostCategoryService.GetCurrentListForBudget(budget.EntityId),
                ReturnUrl = returnUrl,
                ParentId = budgetId
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(VariableCostCreateEditVM viewModel)
        {
            if(viewModel.UserId == Guid.Empty)
            {
                ModelState.AddModelError("UserId", "Please select a user");
            }

            if (viewModel.CategoryId == Guid.Empty)
            {
                ModelState.AddModelError("AvailableCategories", "Please select a category");
            }

            if (viewModel.VariableCostVM.Amount <= 0)
            {
                ModelState.AddModelError("VariableCostVM.Amount", "Amount must be greater than 0");
            }

            if (ModelState.IsValid)
            {
                Budget? budget = _entityServiceManager.BudgetService.GetById(viewModel.ParentId);
                if(budget == null)
                {
                    _logger.LogError("Failed to retrieve budget");
                    return View("Error", GetErrorViewModel(ErrorType.NotFound));
                }

                VariableCost variableCost = new VariableCost()
                {
                    Budget = budget,
                    DateIncurred = viewModel.VariableCostVM.DateIncurred,
                    Amount = viewModel.VariableCostVM.Amount,
                    UserId = viewModel.UserId,
                    Description = viewModel.VariableCostVM.Description,
                    Category = _entityServiceManager.CostCategoryService.GetById(viewModel.CategoryId)
                };

                bool success = _entityServiceManager.VariableCostService.Add(variableCost);

                if (success)
                {
                    _entityServiceManager.SaveChanges();
                    return Redirect(viewModel.ReturnUrl);
                }
                else
                {
                    _logger.LogError("Failed to add variable cost to budget");
                    return View("Error", GetErrorViewModel(ErrorType.OperationFailed));
                }
            }

            _logger.LogError("Model is invalid");
            viewModel.AvailableUsers = _userServiceManager.GetAllUsersForEntity(viewModel.ParentId);
            viewModel.AvailableCategories = _entityServiceManager.CostCategoryService.GetCurrentListForBudget(viewModel.ParentId);
            return View(viewModel);
        }

        public IActionResult Edit(Guid variableCostId, string returnUrl)
        {
            VariableCost? variableCost = _entityServiceManager.VariableCostService.GetById(variableCostId, v => v.Category, v => v.Budget );

            if(variableCost == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            VariableCostCreateEditVM viewModel = new VariableCostCreateEditVM()
            {
                VariableCostVM = new VariableCostVM()
                {
                    VariableCostId = variableCost.EntityId,
                    Amount = variableCost.Amount,
                    Description = variableCost.Description,
                },
                ReturnUrl = returnUrl,
                ParentId = variableCost.Budget.EntityId,
                UserId = variableCost.UserId,
                CategoryId = variableCost.Category.EntityId,
                AvailableUsers = _userServiceManager.GetAllUsersForEntity(variableCost.Budget.EntityId),
                AvailableCategories = _entityServiceManager.CostCategoryService.GetCurrentListForBudget(variableCost.Budget.EntityId)
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(VariableCostCreateEditVM viewModel)
        {
            if (viewModel.UserId == Guid.Empty)
            {
                ModelState.AddModelError("UserId", "Please select a user");
            }

            if (viewModel.CategoryId == Guid.Empty)
            {
                ModelState.AddModelError("AvailableCategories", "Please select a category");
            }

            if (viewModel.VariableCostVM.Amount <= 0)
            {
                ModelState.AddModelError("VariableCostVM.Amount", "Amount must be greater than 0");
            }

            if (ModelState.IsValid)
            {
                VariableCost? variableCost = _entityServiceManager.VariableCostService.GetById(viewModel.VariableCostVM.VariableCostId);
                if(variableCost != null)
                {
                    variableCost.Amount = viewModel.VariableCostVM.Amount;
                    variableCost.Description = viewModel.VariableCostVM.Description;
                    variableCost.UserId = viewModel.UserId;
                    variableCost.DateIncurred = viewModel.VariableCostVM.DateIncurred;
                    variableCost.Category = _entityServiceManager.CostCategoryService.GetById(viewModel.CategoryId);

                    bool success = _entityServiceManager.VariableCostService.Update(variableCost);

                    if(success)
                    {
                        _entityServiceManager.VariableCostService.SaveChanges();
                        return Redirect(viewModel.ReturnUrl);
                    }
                    else
                    {
                        return View("Error", GetErrorViewModel(ErrorType.OperationFailed));
                    }
                }
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            viewModel.AvailableUsers = _userServiceManager.GetAllUsersForEntity(viewModel.ParentId);
            viewModel.AvailableCategories = _entityServiceManager.CostCategoryService.GetCurrentListForBudget(viewModel.ParentId);
            return View(viewModel);
        }

        public IActionResult Remove(Guid variableCostId, string returnUrl) 
        {
            VariableCost? variableCost = _entityServiceManager.VariableCostService.GetById(variableCostId);
            if(variableCost == null)
            {
                return Json(new { title = "Operation Failed", message = "Please contact your administrator for assistance" });
            }

            _entityServiceManager.VariableCostService.Archive(variableCost);
            _entityServiceManager.SaveChanges();

            return Json(new { status = "success", title = "Success", message = "Expense Deleted", returnUrl });
        }
    }
}
