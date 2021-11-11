using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using NetsBrokerIntegration.DotNet.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NetsBrokerIntegration.DotNet.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Signin()
        {
            var auth= HttpContext.GetOwinContext().Authentication;
            var loginInfo = await auth.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return RedirectToAction("Index");
            }

            var id = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
            id.AddClaims(loginInfo.ExternalIdentity.Claims);
            HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties(), id);
            return RedirectToAction("LoggedInSuccess");
        }

        [AllowAnonymous]
        public ActionResult LoginOidc()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Signin", new { ReturnUrl = "/" })
                }, "OpenIdConnect");
                return new HttpUnauthorizedResult();
            }

            return RedirectToAction("LoggedInSuccess");
        }

        [Authorize]
        public ActionResult LoggedInSuccess()
        {
            return View();
        }

        [Authorize]
        public ActionResult Claims()
        {
            ViewBag.UserClaims = ClaimsPrincipal.Current.Identities.First().Claims.ToList();
            return View();
        }

        public ActionResult Error(AuthenticationErrorResponse authenticationErrorResponse)
        {
            return View(authenticationErrorResponse);
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            var user = HttpContext.GetOwinContext().Authentication.User;
            if (user?.Identity.IsAuthenticated == true)
            {
                HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
            return RedirectToAction("Index");
            
        }
    }
}