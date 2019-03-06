using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult UserInfo()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
//                string accessToken = await HttpContext.GetTokenAsync("access_token");
//                string idToken = await HttpContext.GetTokenAsync("id_token");
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var preferredUsername = identity.FindFirst("preferred_username").Value;
            var email = identity.FindFirst(ClaimTypes.Email).Value;
            var displayName = identity.FindFirst("name").Value;

            return View(new UserInfoModel
                {UserId = userId, Email = email, Username = preferredUsername, DisplayName = displayName});
        }


        [Authorize(Roles = "admin")]
        public IActionResult AdminInfo()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
//                string accessToken = await HttpContext.GetTokenAsync("access_token");
//                string idToken = await HttpContext.GetTokenAsync("id_token");
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var preferredUsername = identity.FindFirst("preferred_username").Value;
            var email = identity.FindFirst(ClaimTypes.Email).Value;
            var displayName = identity.FindFirst("name").Value;

            return View(new UserInfoModel
                {UserId = userId, Email = email, Username = preferredUsername, DisplayName = displayName});
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}