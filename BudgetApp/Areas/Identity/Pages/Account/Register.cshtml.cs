// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using BudgetApp.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using BudgetApp.DataServices.ServiceManagers;
using BudgetApp.Models;
using static BudgetApp.Errors;
using BudgetApp.Configurations;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Globalization;

namespace BudgetApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly UserServiceManager _userService;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<User> userManager,
            UserServiceManager userService,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userService = userService;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Birthdate")]
            public DateTime Birthdate { get; set; }


            [Display(Name = "Year")]
            public int Year { get; set; } = DateTime.Now.Year;

            public int Month { get; set; }

            public int Day { get; set; }

            public List<SelectListItem> AvailableYears { get; set; }

            public List<SelectListItem> AvailableMonths { get; set; }

            public List<SelectListItem> AvailableDays { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string InviteToken { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null, string inviteToken = null)
        {
            Input = new InputModel();

            if (inviteToken != null)
            {
                InviteToEntity invite = _userService.GetInviteFromToken(inviteToken, out bool isExpired, out bool invalidToken);

                if(invite != null)
                {
                    Input.InviteToken = inviteToken;
                    Input.Email = invite.InvitedUserEmail;
                    returnUrl = HttpContext.BaseUrl() + Url.Action("AcceptInvite", "Invite", new { inviteToken = invite.Token });
                }
                else //if no invite was returned, evaluate what the cause was
                {
                    ErrorType error;
                    if(isExpired || invalidToken)
                    {
                        error = isExpired ? ErrorType.ExpiredToken : ErrorType.InvalidToken;
                    }
                    else
                    {
                        error = ErrorType.NotFound;
                    }

                    return RedirectToAction("Error", "Error", new { errorType = error });
                }
            }

            Input.AvailableYears = Enumerable.Range(1900, 2023)
                .Select(i => new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString(),
                }).ToList();

            Input.AvailableMonths = Enumerable.Range(1, 12)
                .Select(i => new SelectListItem
                {
                    Value = i.ToString(),
                    Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
                }).ToList();

            Input.AvailableDays = Enumerable.Range(1, 31)
               .Select(i => new SelectListItem
               {
                   Value = i.ToString(),
                   Text = i.ToString()
               }).ToList();

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.BirthDate = new DateTime(Input.Year, Input.Month, Input.Day);

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"{user.FullName} created a new account.");

                    var userId = await _userManager.GetUserIdAsync(user);

                    if(Input.InviteToken == null)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        var adminApprovalLink = HttpContext.BaseUrl() + Url.Action("ApproveRequest", "Admin", new { userEmail = Input.Email });

                        /* REMOVED so that only admins can approve a user who is not previously invited
                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                        */

                        string approvalMessage =  $"{Input.FirstName} {Input.LastName} has registered an account and needs approval." +
                         $"<a href = '{HtmlEncoder.Default.Encode(adminApprovalLink)}'>Approve</a>.";
                        await _emailSender.SendEmailAsync(Credentials.AdminEmail, "New User Has Registered", approvalMessage);
                           

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    else
                    {
                        _userService.ConfirmUser(user);
                        return Redirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            Input.AvailableYears = Enumerable.Range(1900, 2023)
              .Select(i => new SelectListItem
              {
                  Value = i.ToString(),
                  Text = i.ToString(),
              }).ToList();

            Input.AvailableMonths = Enumerable.Range(1, 12)
                .Select(i => new SelectListItem
                {
                    Value = i.ToString(),
                    Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
                }).ToList();

            Input.AvailableDays = Enumerable.Range(1, 31)
               .Select(i => new SelectListItem
               {
                   Value = i.ToString(),
                   Text = i.ToString()
               }).ToList();

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
