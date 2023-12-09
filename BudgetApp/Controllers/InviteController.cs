using BudgetApp.Areas.Identity;
using BudgetApp.Areas.Identity.Models;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.Models;
using BudgetApp.ViewModels.ErrorViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text;
using static BudgetApp.Errors;
using Org.BouncyCastle.Asn1.X509.Qualified;
using BudgetApp.ViewModels.InviteViewModels;

namespace BudgetApp.Controllers
{
    public class InviteController : Controller
    {
        ILogger _logger;
        IEmailSender _emailService;
        EntityServiceManager _entityServiceManager;
        UserServiceManager _userServiceManager;

        public InviteController(ILogger<InviteController> logger, EntityServiceManager entityServiceManager, UserServiceManager userServiceManager, IEmailSender emailService)
        {
            _logger = logger;
            _entityServiceManager = entityServiceManager;
            _userServiceManager = userServiceManager;
            _emailService = emailService;
        }

        public IActionResult InviteUser(Guid entityId, string entityType, string returnUrl)
        {
            Guid? currentUserId = _userServiceManager.GetCurrentUserId();

            if (currentUserId == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthenticated));
            }

            if (_userServiceManager.GetUserRoleForEntity(currentUserId, entityId) != RoleName.Owner)
            {
                return View("Error", GetErrorViewModel(ErrorType.Unauthorized));
            }

            InviteToEntity invite = new InviteToEntity()
            {
                EntityId = entityId,
                EntityType = entityType,
                InviterUserId = (Guid)currentUserId,
                ReturnUrl = returnUrl
            };

            return View(invite);
        }

        [HttpPost]
        public async Task<IActionResult> InviteUser(InviteToEntity invite)
        {
            if (!string.IsNullOrEmpty(invite.InvitedUserEmail))
            {
                //generate random token
                byte[] tokenBytes = new byte[64];
                RandomNumberGenerator.Create().GetBytes(tokenBytes);
                invite.Token = Convert.ToBase64String(tokenBytes);
                invite.ExpirationDate = DateTime.Now.AddMinutes(Configurations.Invitations.MinutesUntilExpiration);

                User? user = await _userServiceManager.GetUserByEmail(invite.InvitedUserEmail);
                string actionLink = string.Empty;

                //no user exists with the inviteEmail, redirect to invitee to registration first
                if (user == null) 
                {
                    actionLink += Url.Page("/Account/Register",
                                  pageHandler: null,
                                  values: new { area = "Identity", inviteToken = invite.Token },
                                  protocol: Request.Scheme);
                }
                else
                {
                    var entity = _userServiceManager.GetEntityForUser<Budget>(user.Id);

                    //user exists, but is not to this entity, redirect to allow them to accept invite
                    if (entity == null) 
                    {
                        actionLink = HttpContext.BaseUrl() + Url.Action("AcceptInvite", "Invite", new { inviteToken = invite.Token });
                    }
                    else //invited user is already part of another entity, don't allow invite
                    {

                        ErrorViewModel errorViewModel = new ErrorViewModel()
                        {
                            Title = Errors.InvalidOperation.Title,
                            Message = "The user you are trying to invite is already associated with another budget",
                            Action = "They must remove themselves from their existing budget to be invited to another"
                        };

                        return View("Error", errorViewModel);
                    }
                }

                bool inviteExists = _userServiceManager.InviteAlreadyExistsForUser(invite);

                if (!inviteExists)
                {
                    _userServiceManager.AddInvitationForUser(invite);

                    try
                    {
                        User? inviterUser = _userServiceManager.GetUserById(invite.InviterUserId);
                        string message = $"<a href='{HtmlEncoder.Default.Encode(actionLink)}'>Click Here</a>";
                        string subject = $"You have an invitation from {inviterUser?.FullName ?? "A Secret Admirer"}";
                        await _emailService.SendEmailAsync(invite.InvitedUserEmail, subject , message);
                    }
                    catch (Exception ex)
                    {
                        StringBuilder errorMessage = new StringBuilder();
                        errorMessage.AppendLine($"Failed to send email invite to user {invite.InvitedUserEmail}" + Environment.NewLine);
                        errorMessage.AppendLine("Details:");
                        errorMessage.AppendLine(ex.Message);
                        _logger.LogError(errorMessage.ToString());

                        ErrorViewModel errorViewModel = new ErrorViewModel()
                        {
                            Title = Errors.OperationFailed.Title,
                            Message = $"The email to {invite.InvitedUserEmail} failed to send",
                            Action = "Please check validity of email and try again. If the problem persists, please contact your administrator"
                        };

                        return View("Error", errorViewModel);
                    }

                    if (invite.ReturnUrl == null)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return Redirect(invite.ReturnUrl);
                    }
                }
                else
                {

                    ErrorViewModel errorViewModel = new ErrorViewModel()
                    {
                        Title = Errors.InvalidOperation.Title,
                        Message = "An invite to this user has already been sent.",
                        Action = "Please wait until the user responds to the invite, or the invite expires"
                    };

                    return View("Error", errorViewModel);
                }
            }

            return View("Error", GetErrorViewModel(ErrorType.OperationFailed));
        }


        public IActionResult AcceptInvite(string inviteToken)
        {
            InviteToEntity? invite = _userServiceManager.GetInviteFromToken(inviteToken, out bool isExpired, out bool invalidToken);

            if(invite == null)
            {
                if(invalidToken)
                {
                    return View("Error", GetErrorViewModel(ErrorType.InvalidToken));
                }
                else if(isExpired)
                {
                    return View("Error", GetErrorViewModel(ErrorType.ExpiredToken));
                }
                else
                {
                    return RedirectToAction("Error", "Error", new ErrorViewModel());
                }
            }

            string inviterName = _userServiceManager.GetUserById(invite.InviterUserId)?.FullName ?? IdentityConstants.UNIDENTIFIED_USER;

            return View(new InviteVM() { InviteId = invite.Id, InviterName = inviterName });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvite(InviteVM viewModel)
        {
            InviteToEntity? invite = _userServiceManager.GetInviteById(viewModel.InviteId);

            if(invite == null)
            {
                return View("Error", GetErrorViewModel(ErrorType.NotFound));
            }

            User? invitedUser = await _userServiceManager.GetUserByEmail(invite.InvitedUserEmail);
            User? inviter = _userServiceManager.GetUserById(invite.InviterUserId);

            if (invitedUser == null)
            {
                return View("Error", new ErrorViewModel() { Action = "You must create an account to accept an invite" });
            }

            if (viewModel.InvitationAccepted)
            {
                bool success = _userServiceManager.AddUserToEntity(invitedUser.Id, invite.EntityId, invite.EntityType, RoleName.User);

                if (success == false)
                {
                    return View("Error", new ErrorViewModel() { Message = "Unable to associate your account to this budget" });
                }
            }

            //Mark invite as expired since it has been used now
            _userServiceManager.ExpireInvite(invite.Id);

            //Send email notification
            string acceptedOrRejected = viewModel.InvitationAccepted ? "accepted" : "rejected";
            string subject = $"Invite {acceptedOrRejected}";
            string message = $"{invitedUser.FullName} has {acceptedOrRejected} your invitation.";
            _logger.LogInformation(message);

            if (inviter?.Email != null)
            {
                try
                {
                    await _emailService.SendEmailAsync(inviter.Email, subject, message);
                }
                catch (Exception ex) 
                {
                    _logger.LogError($"Failed to send email message to {inviter.FullName}");
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
