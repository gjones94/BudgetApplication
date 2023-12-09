using BudgetApp.Areas.Identity.Models;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.Models;
using BudgetApp.ViewModels.ErrorViewModels;
using BudgetApp.ViewModels.HomeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using static BudgetApp.Errors;

namespace BudgetApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private EntityServiceManager _entityServiceManager;
        private UserServiceManager _userService;

        public HomeController(EntityServiceManager entityServiceManager, UserServiceManager userServiceManager, ILogger<HomeController> logger)
        {
            _logger = logger;
            _entityServiceManager = entityServiceManager;
            _userService = userServiceManager;
        }

        public IActionResult Index()
        {
            User? user = _userService.GetCurrentUser();

            if (user == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthenticated));
            }

            Budget? budget = _userService.GetEntityForUser<Budget>(user.Id);

            //Redirect to budget details if it exists
            if (budget is not null)
            {
                return RedirectToAction("Menu", new { budgetId = budget.EntityId });
            }

            return View();
        }

        public IActionResult Menu()
        {
            User ? user = _userService.GetCurrentUser();

            if (user == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthenticated));
            }

            Budget? budget = _userService.GetEntityForUser<Budget>(user.Id);

            if (budget == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            //Redirect to budget details if it exists
            MenuVM viewModel = new MenuVM()
            {
                BudgetId = budget.EntityId,
                ReturnUrl = Url.Action("Menu")
            };

            return View(viewModel);
        }
    }
}