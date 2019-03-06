using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync("Keycloak", new AuthenticationProperties
            {
                RedirectUri = returnUrl
            });
        }

        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Keycloak", new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            });

            await HttpContext.SignOutAsync("Cookies");
        }

        [HttpPost]
        [Route("/keycloak/k_logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task BackChannelLogout()
        {

            // TODO instead of generic Keycloak admin request handling... how about validating the token manually?
            // see https://developer.okta.com/blog/2018/03/23/token-authentication-aspnetcore-complete-guide#validate-tokens-manually-in-aspnet-core
            
            Console.WriteLine("Backchannel logout");
            
            await Task.CompletedTask;
        }
    }
}