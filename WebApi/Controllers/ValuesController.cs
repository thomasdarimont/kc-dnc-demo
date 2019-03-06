using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Ping()
        {
            return "Hello from backend-service";
        }

        [HttpGet("user")]
        [Authorize(Roles = "user")]
        public ActionResult<string> GetUserInfo()
        {
            return "Hello from backend-service - only with role USER";
        }

        [HttpGet("admin")]
        [Authorize(Roles = "admin")]
        public ActionResult<string> GetAdminInfo()
        {
            return "Hello from backend-service - only with role ADMIN";
        }
    }
}