using BudgetApp.Areas.Identity;
using BudgetApp.Areas.Identity.Models;
using BudgetApp.Configurations;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.ViewModels.AdminViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Bcpg;
using System.Text.Encodings.Web;
using static BudgetApp.Errors;

namespace BudgetApp.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {

        ILogger _logger;
        EntityServiceManager _entityServiceManager;
        IEmailSender _emailSender;
        UserServiceManager _userServiceManager;

        public AdminController(ILogger<AdminController> logger, EntityServiceManager entityServiceManager, UserServiceManager userServiceManager, IEmailSender emailService)
        {
            _logger = logger;
            _entityServiceManager = entityServiceManager;
            _emailSender = emailService;
            _userServiceManager = userServiceManager;
        }

        [HttpGet]
        public async Task<IActionResult> ApproveRequest(string userEmail)
        {
            User user = await _userServiceManager.GetUserByEmail(userEmail);

            if(user == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            ApprovalVM viewModel = new ApprovalVM()
            {
                UserId = user.Id,
                FullName = user.FullName
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ApproveResponse(Guid userId, bool approved)
        {
            User currentUser  = _userServiceManager.GetCurrentUser();
            bool userIsAdmin = await _userServiceManager.IsInRole(currentUser.Id, RoleName.Admin);

            if(userIsAdmin)
            {
                User user = _userServiceManager.GetUserById(userId);

                if (user == null)
                {
                    return View("Error", GetErrorViewModel(ErrorType.NotFound));
                }

                if(approved)
                {
                    user.ApprovedByAdmin = true;
                    user.EmailConfirmed = true;
                    _userServiceManager.SaveChanges();
                    string approvalLink = HttpContext.BaseUrl() + Url.Action("Index", "Home");

                    string message = $"Your account has been approved! <a href='{HtmlEncoder.Default.Encode(approvalLink)}'>Login</a>";
                    await _emailSender.SendEmailAsync(user.Email, "Registration Approved", message);
                }
                else
                {
                    bool success = await _userServiceManager.DeleteUserAccount(user);
                    if(success) 
                    {
                        await _emailSender.SendEmailAsync(user.Email, "Registration Rejected", "Administrator has not approved your access. Your account has been deleted.");
                    }
                    else
                    {
                        await _emailSender.SendEmailAsync(Credentials.AdminEmail, $"Failed to Delete User Account: {user.FullName}", "Please delete manually");
                    }
                }
            }
            else
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthorized));
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
