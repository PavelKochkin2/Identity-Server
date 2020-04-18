using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Basics.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDataProtectionProvider _provider;

        public HomeController(IDataProtectionProvider provider)
        {
            _provider = provider;
        }

        public IActionResult Index()
        {
            return View();
        }


        [Authorize]
        public IActionResult Secret()
        {

            var s = GetClaimFromCookie(HttpContext, "Grandmas.Cookie", "CookieAuth");

            return View();

        }
        private IEnumerable<Claim> GetClaimFromCookie(HttpContext httpContext, string cookieName, string cookieSchema)
        {
            // Get the encrypted cookie value
            var opt = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
            var cookie = opt.CurrentValue.CookieManager.GetRequestCookie(httpContext, cookieName);

            // Decrypt if found
            if (!string.IsNullOrEmpty(cookie))
            {
                var dataProtector = opt.CurrentValue.DataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", cookieSchema, "v2");

                var ticketDataFormat = new TicketDataFormat(dataProtector);
                var ticket = ticketDataFormat.Unprotect(cookie);
                return ticket.Principal.Claims;
            }
            return null;
        }

        public IActionResult Authenticate()
        {
            var gradmaClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Pashok"),
                new Claim(ClaimTypes.Email, "top@mail.com"),
                new Claim("Grandma.Says", "Very nice boi")
            };

            var licenceClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Pavel Kochkin"),
                new Claim("Driving Licence", "A+"),
            };

            var grandmaIdentity = new ClaimsIdentity(gradmaClaims, "Grandma Identity");
            var licenceIdentity = new ClaimsIdentity(licenceClaims, "Government");


            var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity, licenceIdentity });

            HttpContext.SignInAsync(userPrincipal);

            var context = HttpContext;


            var user = HttpContext.User;

            return RedirectToAction(nameof(Index));
        }
    }
}