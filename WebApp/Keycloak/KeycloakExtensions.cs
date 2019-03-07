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
            Action<OpenIdConnectOptions> configureDefaultOptions = keycloakOptions =>
            {
                keycloakOptions.ResponseType = "code";

                // Configure the scope
                keycloakOptions.Scope.Clear();
                keycloakOptions.Scope.Add("openid");
                keycloakOptions.Scope.Add("profile");

                keycloakOptions.CallbackPath = new PathString(KeycloakDefaults.CallbackPath);
                keycloakOptions.SignedOutCallbackPath = new PathString(KeycloakDefaults.SignedOutCallbackPath);

                keycloakOptions.RequireHttpsMetadata = true;
                keycloakOptions.SaveTokens = true;
                keycloakOptions.SignedOutRedirectUri = KeycloakDefaults.SignedOutRedirectUri;

                keycloakOptions.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = context =>
                    {
                        var resourceAccess = JObject.Parse(context.Principal.FindFirst("resource_access").Value);
                        var clientResource = resourceAccess[context.Options.ClientId];
                        var clientRoles = clientResource["roles"];
                        var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                        if (claimsIdentity == null)
                        {
                            return Task.CompletedTask;
                        }

                        foreach (var clientRole in clientRoles)
                        {
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, clientRole.ToString()));
                        }

                        return Task.CompletedTask;
                    },

                    OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.SetParameter("audience", keycloakOptions.ClientId);

                        return Task.FromResult(0);
                    }
                };
            };

            return builder.AddOpenIdConnect(KeycloakDefaults.AuthenticationScheme, options =>
            {
                configureDefaultOptions.Invoke(options);
                configureOptions.Invoke(options);
            });
        }

        public static AuthenticationBuilder AddKeycloakTokenManagement(
            this AuthenticationBuilder builder
        )
        {
            return AddKeycloakTokenManagement(builder, (configureOptions) => { });
        }

        public static AuthenticationBuilder AddKeycloakTokenManagement(
            this AuthenticationBuilder builder,
            Action<AutomaticTokenManagementOptions> configureOptions
        )
        {
            Action<AutomaticTokenManagementOptions> configureDefaultOptions = automaticTokenManagementOptions =>
            {
                automaticTokenManagementOptions.Scheme = KeycloakDefaults.AuthenticationScheme;
            };

            return builder.AddAutomaticTokenManagement(options =>
            {
                configureDefaultOptions.Invoke(options);
                configureOptions.Invoke(options);
            });
        }
    }
}