using BudgetApp.DataServices;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.Models;
using BudgetApp.ViewModels.CostCategoryViewModels;
using Microsoft.AspNetCore.Mvc;
using static BudgetApp.Errors;

namespace BudgetApp.Controllers
{
    public class CostCategoryController : Controller
    {
        ILogger _logger;
        EntityServiceManager _entityServiceManager;
        UserServiceManager _userServiceManager;

        public CostCategoryController(ILogger<CostCategoryController> logger, EntityServiceManager entityServiceManager, UserServiceManager userServiceManager)
        {
            _logger = logger;
            _entityServiceManager = entityServiceManager;
            _userServiceManager = userServiceManager;
        }

        public IActionResult Create(Guid budgetId, string returnUrl)
        {
            Budget? budget = _entityServiceManager.BudgetService.GetById(budgetId);

            if (budget == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            CostCategoryCreateEditVM viewModel = new CostCategoryCreateEditVM()
            {
                CostCategoryVM = new CostCategoryVM() { CostCategoryId = Guid.NewGuid() },
                ReturnUrl = returnUrl,
                ParentId = budgetId
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(CostCategoryCreateEditVM viewModel)
        {
            if(viewModel.CostCategoryVM.BudgetedAmount <= 0)
            {
                ModelState.AddModelError("CostCategoryVM.BudgetedAmount", "Amount must be greater than 0");
            }

            double amountLeftToAllocate = _entityServiceManager.BudgetService.GetRemainingCategoryAllocationForBudget(viewModel.ParentId);

            if(viewModel.CostCategoryVM.BudgetedAmount > amountLeftToAllocate)
            {
                string error = (amountLeftToAllocate <= 0) ? "You do not have any remaining funds to allocate" : $"You only have {amountLeftToAllocate} remaining to allocate";
                if (amountLeftToAllocate <= 0)
                {
                    ModelState.AddModelError("CostCategoryVM.BudgetedAmount", error);
                }

                ModelState.AddModelError("CostCategoryVM.BudgetedAmount", error);
            }

            if (ModelState.IsValid)
            {
                Budget? budget = _entityServiceManager.BudgetService.GetById(viewModel.ParentId);

                if (budget == null)
                {
                    _logger.LogError("Failed to retrieve budget");
                    return View("Error", GetErrorViewModel(ErrorType.NotFound));
                }

                CostCategory costCategory = new CostCategory()
                {
                    Budget = budget,
                    BudgetedAmount = viewModel.CostCategoryVM.BudgetedAmount,
                    Name = viewModel.CostCategoryVM.Name
                };

                bool success = _entityServiceManager.CostCategoryService.Add(costCategory);
                if (success)
                {
                    _entityServiceManager.SaveChanges();
                    return Redirect(viewModel.ReturnUrl);
                }
                else
                {
                    _logger.LogError("Failed to add cost category to budget");
                    return View("Error", GetErrorViewModel(ErrorType.OperationFailed));
                }
            }

            _logger.LogError("Model is invalid");
            return View(viewModel);
        }

        public IActionResult Edit(Guid costCategoryId, string returnUrl)
        {
            CostCategory? costCategory = _entityServiceManager.CostCategoryService.GetById(costCategoryId, relatedPropertiesToLoad: c => c.Budget);

            if (costCategory == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            CostCategoryCreateEditVM viewModel = new CostCategoryCreateEditVM()
            {
                CostCategoryVM = new CostCategoryVM()
                {
                    CostCategoryId = costCategory.EntityId,
                    Name = costCategory.Name,
                    BudgetedAmount = costCategory.BudgetedAmount
                },
                ReturnUrl = returnUrl,
                ParentId = costCategory.Budget.EntityId,
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(CostCategoryCreateEditVM viewModel)
        {
            if (viewModel.CostCategoryVM.BudgetedAmount <= 0)
            {
                ModelState.AddModelError("CostCategoryVM.BudgetedAmount", "Amount must be greater than 0");
            }

            if (ModelState.IsValid)
            {
                CostCategory? costCategory = _entityServiceManager.CostCategoryService.GetById(viewModel.CostCategoryVM.CostCategoryId);

                if(costCategory != null)
                {
                    costCategory.Name = viewModel.CostCategoryVM.Name;
                    costCategory.BudgetedAmount = viewModel.CostCategoryVM.BudgetedAmount;

                    bool success = _entityServiceManager.CostCategoryService.Update(costCategory);
                    if (success)
                    {
                        _entityServiceManager.CostCategoryService.SaveChanges();
                        return Redirect(viewModel.ReturnUrl);
                    }
                    else
                    {
                        return View("Error", GetErrorViewModel(ErrorType.OperationFailed));
                    }
                }

                return View("Error", GetErrorViewModel(ErrorType.NotFound));

                
            }

            return View(viewModel);
        }

        public IActionResult Remove(Guid costCategoryId, string returnUrl)
        {
            CostCategory? costCategory = _entityServiceManager.CostCategoryService.GetById(costCategoryId);

            if (costCategory == null)
            {
                return Json(new { title = "Operation Failed", message = "Please contact your administrator for assistance" });
            }

            _entityServiceManager.CostCategoryService.Archive(costCategory);
            _entityServiceManager.SaveChanges();

            return Json( new { status = "success", title = "Success", message = "Category Deleted", returnUrl });
        }
    }
}
