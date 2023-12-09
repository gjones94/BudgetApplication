using BudgetApp.Areas.Identity;
using BudgetApp.Areas.Identity.Models;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.Models;
using BudgetApp.ViewModels.ErrorViewModels;
using BudgetApp.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static BudgetApp.Errors;

namespace BudgetApp.Controllers
{
    public class UserController : Controller
    {
        ILogger _logger;
        IEmailSender _emailService;
        EntityServiceManager _entityServiceManager;
        UserServiceManager _userServiceManager;

        /// <summary>
        /// Handles the modification of user access to entities
        /// </summary>
        public UserController(ILogger<UserController> logger, EntityServiceManager entityServiceManager, UserServiceManager userServiceManager, IEmailSender emailService)
        {
            _logger = logger;
            _entityServiceManager = entityServiceManager;
            _userServiceManager = userServiceManager;
            _emailService = emailService;
        }

        public IActionResult Edit(Guid userId, Guid entityId, string returnUrl)
        {
            //Check if user has owner permission
            UserToEntity? userToEntity = _userServiceManager.GetUserToEntity(userId, entityId);

            if(userToEntity == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            string userFullName = _userServiceManager.GetUserFullName(userId);

            UserRoleEditVM viewModel = new UserRoleEditVM()
            {
                UserId = userId,
                Name = userFullName,
                EntityId = entityId,
                SelectedRole = userToEntity.Role ?? "None",
                ReturnUrl = returnUrl,
                AvailableRoles = RoleProvider.GetNonAdminRoles()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(UserRoleEditVM viewModel)
        {
            //Check current user authenticated
            User? currentUser = _userServiceManager.GetCurrentUser();
            if (currentUser == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthenticated));
            }

            //Check current user authorization
            if(_userServiceManager.UserIsAuthorizedForAction(currentUser.Id, viewModel.EntityId, UserAction.ModifyUser) == false)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthorized));
            }

            //Check if relationship exists
            UserToEntity? userToEntity = _userServiceManager.GetUserToEntity(viewModel.UserId, viewModel.EntityId);
            if(userToEntity == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            if(_userServiceManager.UserIsOwnerOfEntity(viewModel.UserId, viewModel.EntityId))
            {
                return View("Error", new ErrorViewModel() { Title = Errors.InvalidOperation.Title, Message = "The user you are trying to modify is an owner. Owner permissions cannot be modified by anyone" });
            }

            userToEntity.Role = viewModel.SelectedRole;
            _userServiceManager.SaveChanges();

            return Redirect(viewModel.ReturnUrl);
        }

        public IActionResult Remove(Guid userId, Guid entityId, string returnUrl)
        {
            //Check current user authenticated
            User? currentUser = _userServiceManager.GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { title = Errors.Unauthenticated.Title, message = Errors.Unauthenticated.Message });
            }

            //Check current user authorization
            if (!_userServiceManager.UserIsAuthorizedForAction(currentUser.Id, entityId, UserAction.ModifyUser))
            {
                return Json(new { title = Errors.Unauthorized.Title, message = Errors.Unauthorized.Message });
            }

            //Check to see if user being removed is an owner
            if (_userServiceManager.UserIsOwnerOfEntity(userId, entityId))
            {
                return Json( new { title = Errors.InvalidOperation.Title, Message = "The user you are trying to modify is an owner. Owner permissions cannot be modified by anyone" });
            }

            bool success = _userServiceManager.Remove(userId, entityId);

            if(success)
            {
                _userServiceManager.SaveChanges();
                return Json(new { status = "success", title = "Success", message = "User Removed From Budget", returnUrl });
            }
            else
            {
                return Json(new { title = Errors.OperationFailed.Title, message = Errors.OperationFailed.Message });
            }
        }
    }
}
