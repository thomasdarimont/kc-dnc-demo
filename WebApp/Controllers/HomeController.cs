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
using Microsoft.Extensions.Configuration;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _backendUrl;

        public HomeController(IConfiguration configuration)
        {
            _backendUrl = configuration["BackendService:baseUrl"];
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

            var userData = await CallThirdPartyServiceWithBearerAsync(accessToken, $"{_backendUrl}/user");
            var adminData = await CallThirdPartyServiceWithBearerAsync(accessToken, $"{_backendUrl}/admin");

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


        private async Task<string> CallThirdPartyServiceWithBearerAsync(string accessToken, string url)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                //hack to get around self-signed cert errors in dev
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                    try
                    {
                        var content = await httpClient.GetStringAsync(url);
                        return content;
                    }
                    catch (Exception e)
                    {
                        return "tilt: " + e;
                    }
                }
            }
        }
    }
}