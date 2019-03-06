using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace WebApi.Jwt
{
    public static class JwtConfiguration
    {
        public static void ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("Jwt");
            string audience = config.GetValue<string>("Audience");
            string issuer = config.GetValue<string>("Issuer");
//            TimeSpan tokenExpirationTime = TimeSpan.FromMinutes(config.GetValue<int>("TokenExpirationTimeMinutes", 1));

            services.AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer(options =>
                {
                    options.Audience = audience;
                    options.Authority = issuer;
                    options.RequireHttpsMetadata = false;
                    options.IncludeErrorDetails = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateLifetime = true
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            // Extract Keycloak Client Roles from JWT
                            
                            var resourceAccess = JObject.Parse(context.Principal.FindFirst("resource_access").Value);
                            var clientResource = resourceAccess[context.Principal.FindFirstValue("aud")];
                            var clientRoles = clientResource["roles"];
                            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                            
                            foreach (var clientRole in clientRoles)
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, clientRole.ToString()));
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
        }

        public static void ConfigureJwtAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });
        }
    }
}