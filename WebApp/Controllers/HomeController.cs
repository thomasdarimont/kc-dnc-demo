using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Backend;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBackendService _backendService;

        public HomeController(IBackendService backendService)
        {
            _backendService = backendService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult UserInfo()
        {
            return View(UserInfoModel.Create(HttpContext.User.Identity as ClaimsIdentity));
        }


        [Authorize(Roles = "admin")]
        public IActionResult AdminInfo()
        {
            return View(UserInfoModel.Create(HttpContext.User.Identity as ClaimsIdentity));
        }

        public async Task<IActionResult> BackendService()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var userData = await _backendService.GetUserDataAsync();
            var adminData = await _backendService.GetAdminDataAsync();

            return View(new ApiResponseModel {UserData = userData, AdminData = adminData});
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = exceptionHandlerPathFeature?.Error.Message,
                RequestPath = exceptionHandlerPathFeature?.Path
            });
        }
    }
}