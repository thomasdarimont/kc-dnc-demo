using System.Security.Claims;

namespace WebApp.Models
{
    public class UserInfoModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }

        public static UserInfoModel Create(ClaimsIdentity identity)
        {
            return new UserInfoModel
            {
                UserId = identity.FindFirst(ClaimTypes.NameIdentifier).Value,
                Username = identity.FindFirst("preferred_username").Value,
                Email = identity.FindFirst(ClaimTypes.Email).Value,
                DisplayName = identity.FindFirst("name").Value
            };
        }
    }
}