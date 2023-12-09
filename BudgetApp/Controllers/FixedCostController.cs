using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.Models;
using BudgetApp.ViewModels.FixedCostViewModels;
using BudgetApp.ViewModels.IncomeViewModels;
using Microsoft.AspNetCore.Mvc;
using static BudgetApp.Errors;

namespace BudgetApp.Controllers
{
    public class FixedCostController : Controller
    {
        ILogger _logger;
        EntityServiceManager _entityServiceManager;
        UserServiceManager _userServiceManager;

        public FixedCostController(ILogger<FixedCostController> logger, EntityServiceManager entityServiceManager, UserServiceManager userServiceManager)
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

            FixedCostCreateEditVM viewModel = new FixedCostCreateEditVM()
            {
                FixedCostVM = new FixedCostVM(),
                ReturnUrl = returnUrl,
                ParentId = budgetId
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(FixedCostCreateEditVM viewModel)
        {
            if (viewModel.FixedCostVM.Amount <= 0)
            {
                ModelState.AddModelError("FixedCostVM.Amount", "Amount must be greater than 0");
            }

            if (ModelState.IsValid)
            {
                Budget? budget = _entityServiceManager.BudgetService.GetById(viewModel.ParentId);
                if (budget == null)
                {
                    _logger.LogError("Failed to retrieve budget");
                    return View("Error", GetErrorViewModel(ErrorType.NotFound));
                }

                FixedCost fixedCost = new FixedCost()
                {
                    Budget = budget,
                    Amount = viewModel.FixedCostVM.Amount,
                    Description = viewModel.FixedCostVM.Description,
                    MonthBegin = DateTime.Now.GetMonthBegin(),
                    MonthEnd = DateTime.Now.GetMonthEnd()
                };

                bool success = _entityServiceManager.FixedCostService.Add(fixedCost);
                if (success)
                {
                    _entityServiceManager.SaveChanges();
                    return Redirect(viewModel.ReturnUrl);
                }
                else
                {
                    _logger.LogError("Failed to add fixed cost to budget");
                    return View("Error", GetErrorViewModel(ErrorType.OperationFailed));
                }
            }

            _logger.LogError("Model is invalid");
            return View(viewModel);
        }

        public IActionResult Edit(Guid fixedCostId, string returnUrl)
        {
            FixedCost? fixedCost = _entityServiceManager.FixedCostService.GetById(fixedCostId, relatedPropertiesToLoad: f => f.Budget);

            if (fixedCost == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            FixedCostCreateEditVM viewModel = new FixedCostCreateEditVM()
            {
                FixedCostVM = new FixedCostVM()
                {
                    FixedCostId = fixedCost.EntityId,
                    Amount = fixedCost.Amount,
                    Description = fixedCost.Description
                },
                ReturnUrl = returnUrl,
                ParentId = fixedCost.Budget.EntityId,
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(FixedCostCreateEditVM viewModel)
        {
            if (viewModel.FixedCostVM.Amount <= 0)
            {
                ModelState.AddModelError("FixedCostVM.Amount", "Amount must be greater than 0");
            }

            if (ModelState.IsValid)
            {
                FixedCost? fixedCost = _entityServiceManager.FixedCostService.GetById(viewModel.FixedCostVM.FixedCostId);

                if(fixedCost != null)
                {
                    fixedCost.Amount = viewModel.FixedCostVM.Amount;
                    fixedCost.Description = viewModel.FixedCostVM.Description;

                    bool success = _entityServiceManager.FixedCostService.Update(fixedCost);
                    if (success)
                    {
                        _entityServiceManager.SaveChanges();
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

        public IActionResult Remove(Guid fixedCostId, string returnUrl)
        {
            FixedCost? fixedCost = _entityServiceManager.FixedCostService.GetById(fixedCostId);

            if (fixedCost == null)
            {
                return Json(new { title = "Operation Failed", message = "Please contact your administrator for assistance" });
            }

            _entityServiceManager.FixedCostService.Archive(fixedCost);
            _entityServiceManager.SaveChanges();

            return Json(new { status = "success", title = "Success", message = "Fixed Cost Deleted", returnUrl });
        }
    }
}
