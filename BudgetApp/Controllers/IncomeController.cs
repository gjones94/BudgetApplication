using BudgetApp.DataServices;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.Models;
using BudgetApp.ViewModels.IncomeViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static BudgetApp.Errors;

namespace BudgetApp.Controllers
{
    public class IncomeController : Controller
    {
        ILogger _logger;
        EntityServiceManager _entityServiceManager;
        UserServiceManager _userServiceManager;

        public IncomeController(ILogger<IncomeController> logger, EntityServiceManager entityServiceManager, UserServiceManager userServiceManager) 
        {
            _logger = logger;
            _entityServiceManager = entityServiceManager;
            _userServiceManager = userServiceManager;
        }

        public IActionResult Create(Guid budgetId, string returnUrl)
        {
            Budget? budget = _entityServiceManager.BudgetService.GetById(budgetId);

            if(budget == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            IncomeCreateEditVM viewModel = new IncomeCreateEditVM()
            {
                IncomeVM = new IncomeVM(),
                AvailableUsers = _userServiceManager.GetAllUsersForEntity(budget.EntityId),
                ReturnUrl = returnUrl,
                ParentId = budgetId
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(IncomeCreateEditVM viewModel)
        {
            if(viewModel.UserId == Guid.Empty)
            {
                ModelState.AddModelError("AvailableUsers", "Please select a user");
            }

            if(viewModel.IncomeVM.Amount <= 0)
            {
                ModelState.AddModelError("IncomeVM.Amount", "Amount must be greater than 0");
            }

            if (ModelState.IsValid)
            {
                Budget? budget = _entityServiceManager.BudgetService.GetById(viewModel.ParentId);

                if(budget == null)
                {
                    _logger.LogError("Failed to retrieve budget");
                    return View("Error", GetErrorViewModel(ErrorType.NotFound));
                }

                Income income = new Income()
                {
                    Budget = budget,
                    Amount = viewModel.IncomeVM.Amount,
                    UserId = viewModel.UserId,
                    Recurring = viewModel.IncomeVM.Recurring,
                    Description = viewModel.IncomeVM.Description,
                    MonthBegin = DateTime.Now.GetMonthBegin(),
                    MonthEnd = DateTime.Now.GetMonthEnd()
                };

                bool success = _entityServiceManager.IncomeService.Add(income);

                if (success)
                {
                    _entityServiceManager.SaveChanges();
                    return Redirect(viewModel.ReturnUrl);
                }
                else
                {
                    _logger.LogError("Failed to add income to budget");
                    return View("Error", GetErrorViewModel(ErrorType.OperationFailed));
                }
            }

            _logger.LogError("Model is invalid");
            viewModel.AvailableUsers = _userServiceManager.GetAllUsersForEntity(viewModel.ParentId);
            return View(viewModel);
        }

        public IActionResult Edit(Guid incomeId, string returnUrl)
        {

            Income? income = _entityServiceManager.IncomeService.GetById(incomeId, i => i.Budget);

            if(income != null)
            {
                IncomeCreateEditVM viewModel = new IncomeCreateEditVM()
                {
                    IncomeVM = new IncomeVM()
                    {
                        IncomeId = income.EntityId,
                        Amount = income.Amount,
                        Description = income.Description,
                        Recurring = income.Recurring
                    },

                    UserId = income.UserId,
                    ReturnUrl = returnUrl,
                    ParentId = income.Budget.EntityId,
                    AvailableUsers = _userServiceManager.GetAllUsersForEntity(income.Budget.EntityId)
                };

                return View(viewModel);
            }

            return View("Error", GetErrorViewModel(ErrorType.NotFound));
        }

        [HttpPost]
        public IActionResult Edit(IncomeCreateEditVM viewModel)
        {
            if (viewModel.UserId == Guid.Empty)
            {
                ModelState.AddModelError("AvailableUsers", "Please select a user");
            }

            if (viewModel.IncomeVM.Amount <= 0)
            {
                ModelState.AddModelError("IncomeVM.Amount", "Amount must be greater than 0");
            }

            Income? income = _entityServiceManager.IncomeService.GetById(viewModel.IncomeVM.IncomeId);

            if (ModelState.IsValid)
            {
                if(income != null)
                {
                    income.Amount = viewModel.IncomeVM.Amount;
                    income.UserId = viewModel.UserId;
                    income.Description = viewModel.IncomeVM.Description;
                    income.Recurring = viewModel.IncomeVM.Recurring;

                    bool success = _entityServiceManager.IncomeService.Update(income);

                    if(success)
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

            //reload available users
            viewModel.AvailableUsers = _userServiceManager.GetAllUsersForEntity(viewModel.ParentId);

            return View(viewModel);
        }

        public IActionResult Remove(Guid incomeId, string returnUrl) 
        {
            Income? income = _entityServiceManager.IncomeService.GetById(incomeId);
            if(income == null)
            {
                return Json(new { title = OperationFailed.Title, message = "The income could not be deleted" });
            }

            _entityServiceManager.IncomeService.Archive(income);
            _entityServiceManager.SaveChanges();

            return Json(new { status = "success", title = "Success", message = $"Income {income.Description} Deleted", returnUrl });
        }
    }
}
