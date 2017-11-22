using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Web.Security;
using Web.Areas.Account.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using IdentityServer4.Test;
using IdentityServer4.Events;
using Microsoft.AspNetCore.Authorization;

namespace Web.Areas.Account.Controllers
{
    [Area("Account")]
    [SecurityHeaders]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly IAuthRepository _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        public LoginController(
           IIdentityServerInteractionService interaction,
           IClientStore clientStore,
           IHttpContextAccessor httpContextAccessor,
           IAuthenticationSchemeProvider schemeProvider,
           IEventService events,
           IAuthRepository users)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            _users = users;
            _interaction = interaction;
            _events = events;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl)
        {
            return View(new LoginViewModel { EnableLocalLogin = true });
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginInputModel model, string button)
        {

            if (ModelState.IsValid)
            {
                if (_users.ValidatePassword(model.Username, model.Password))
                {
                    var user = _users.GetUserByUsername(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.Login, user.Id.ToString(), user.UserName));

                    // only set explicit expiration here if user chooses "remember me". 
                    // otherwise we rely upon expiration configured in cookie middleware.
                    AuthenticationProperties props = null;
                    model.RememberLogin = true;
                    if (model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(30))
                        };
                    };

                    // issue authentication cookie with subject ID and username
                    await HttpContext.SignInAsync(user.Id.ToString(), user.UserName, props);

                    // make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
                    if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return Redirect("~/");
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));

                // ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            return View(model);
        }
    }
}