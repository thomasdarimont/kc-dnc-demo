using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace WebApp.Keycloak
{
    public static class KeycloakExtensions
    {
        public static AuthenticationBuilder AddKeycloakAuthentication(
            this AuthenticationBuilder builder,
            Action<OpenIdConnectOptions> configureOptions
        )
        {
            return builder.AddOpenIdConnect(KeycloakDefaults.AuthenticationScheme, options =>
            {
                ApplyDefaultKeycloakConfiguration(options);
                configureOptions.Invoke(options);
            });
        }

        public static AuthenticationBuilder AddKeycloakTokenManagement(
            this AuthenticationBuilder builder
        )
        {
            return AddKeycloakTokenManagement(builder, _ => { });
        }

        public static AuthenticationBuilder AddKeycloakTokenManagement(
            this AuthenticationBuilder builder,
            Action<AutomaticTokenManagementOptions> configureOptions
        )
        {
            return builder.AddAutomaticTokenManagement(options =>
            {
                ApplyDefaultKeycloakTokenManagementConfiguration(options);
                configureOptions.Invoke(options);
            });
        }

        private static void ApplyDefaultKeycloakTokenManagementConfiguration(
            AutomaticTokenManagementOptions options)
        {
            options.Scheme = KeycloakDefaults.AuthenticationScheme;
        }

        private static void ApplyDefaultKeycloakConfiguration(OpenIdConnectOptions options)
        {
            options.ResponseType = KeycloakDefaults.ResponseType;

            options.CallbackPath = new PathString(KeycloakDefaults.CallbackPath);
            options.SignedOutCallbackPath = new PathString(KeycloakDefaults.SignedOutCallbackPath);

            options.RequireHttpsMetadata = true;
            options.SaveTokens = true;
            options.SignedOutRedirectUri = KeycloakDefaults.SignedOutRedirectUri;

            options.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = context =>
                {
                    MapKeycloakRolesToRoleClaims(context);
                    return Task.CompletedTask;
                },

                OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.SetParameter("audience", options.ClientId);
                    return Task.FromResult(0);
                }
            };
        }

        private static void MapKeycloakRolesToRoleClaims(TokenValidatedContext context)
        {
            var resourceAccess = JObject.Parse(context.Principal.FindFirst("resource_access").Value);
            var clientResource = resourceAccess[context.Options.ClientId];
            var clientRoles = clientResource["roles"];
            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                return;
            }

            foreach (var clientRole in clientRoles)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, clientRole.ToString()));
            }
        }
    }
}