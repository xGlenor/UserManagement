// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UserManagement.Helper;
using UserManagement.Models;
using UserManagement.Repository;
using UserManagement.Services;

namespace UserManagement.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<LoginModel> _logger;
        private readonly ILogService _logService;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, IUserRepository userRepository, UserManager<ApplicationUser> userManager, ILogService logService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userRepository = userRepository;
            _userManager = userManager;
            _logService = logService;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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

            public string UserName { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(Input.UserName);
               

                if (user != null)
                {

                    if (await _userRepository.GetUserSettingsById(user.Id) == null)
                        await _userRepository.CreateUserSettings(user);
                    
                    var userSettings = await _userRepository.GetUserSettingsById(user.Id);
                    
                    
                    if (user.LastLogin.HasValue && userSettings != null && UserHelper.GetLastLoginDays(user.LastLogin.Value) > userSettings.PasswordExpirationDays)
                    {
                        _logService.CreateLog(user, "USER LOGIN", "ERROR", "User password expired or first login");   
                        ModelState.AddModelError(string.Empty, "Your Password has Expired or First login. You have to reset your password.");
                        return Page();
                    }
                    
                    var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        await _userRepository.UpdateLastLogin(user.Id);
                        _logService.CreateLog(user, "USER LOGIN", "SUCCESS", "User logged in");   
                        _logger.LogInformation("User logged in.");
                        return LocalRedirect(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logService.CreateLog(user, "USER LOGIN", "ERROR", "User acccount locked out.");   
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    _logService.CreateLog(user, "USER LOGIN", "ERROR", "User password is incorrect.");   
                    ModelState.AddModelError(string.Empty, "Login lub Hasło niepoprawny");
                    return Page();
                }
                  
                ModelState.AddModelError(string.Empty, "Login lub Hasło niepoprawny");
                return Page();
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
