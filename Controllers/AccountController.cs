using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Mvc;

namespace Manage_CLB_HTSV.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        public IActionResult Logout()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme, MicrosoftAccountDefaults.AuthenticationScheme);
        }
    }
}
