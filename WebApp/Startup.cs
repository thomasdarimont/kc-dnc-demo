using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect("Keycloak", options =>
                {
                    options.Authority = Configuration["Authentication:oidc:Authority"];
                    options.ClientId = Configuration["Authentication:oidc:ClientId"];
                    options.ClientSecret = Configuration["Authentication:oidc:ClientSecret"];
                    options.ResponseType = "code";

                    // Configure the scope
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.CallbackPath = new PathString("/oauth/callback");
                    options.SignedOutCallbackPath = new PathString("/oauth/logout");

                    options.RequireHttpsMetadata = false;
                    options.SaveTokens = true;
                    options.RemoteSignOutPath = "/keycloak/k_logout";
                    options.SignedOutRedirectUri = "/";

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var resourceAccess = JObject.Parse(context.Principal.FindFirst("resource_access").Value);
                            var clientResource = resourceAccess[context.Options.ClientId];
                            var clientRoles = clientResource["roles"];
                            var currentIdentity = (ClaimsIdentity) context.Principal.Identity;

                            foreach (var clientRole in clientRoles)
                            {
                                currentIdentity.AddClaim(new Claim(ClaimTypes.Role, clientRole.ToString()));
                            }

                            return Task.CompletedTask;
                        },

                        OnRemoteSignOut = async context =>
                        {
                            using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                            {
                                // TODO naive attempt to get logout admin request ... one needs to do a little bit more...
                                var jsonData = await reader.ReadToEndAsync();
                                var klogoutRequest = JObject.Parse(jsonData);
                                Console.WriteLine(jsonData);
                            }

                            await Task.CompletedTask;
                        },

                        OnRedirectToIdentityProvider = context =>
                        {
                            context.ProtocolMessage.SetParameter("audience",
                                Configuration["Authentication:oidc:ClientId"]);

                            return Task.FromResult(0);
                        }
                    };
                });
                
                // TODO implement required infrastructure to support Keycloak Admin Requests...
//                .AddJwtBearer(async options =>
//                {
//                    // TODO how-to use this information from keycloak OIDC configuration?
//                    var httpDocumentRetriever = new HttpDocumentRetriever();
//
//                    // TODO setup everything with HTTPS...
//                    httpDocumentRetriever.RequireHttps = false;
//
//                    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
//                        Configuration["Authentication:oidc:Authority"] + "/.well-known/openid-configuration",
//                        new OpenIdConnectConfigurationRetriever(),
//                        httpDocumentRetriever);
//
//                    var discoveryDocument = await configurationManager.GetConfigurationAsync();
//
//                    options.Authority = Configuration["Authentication:oidc:Authority"];
//                    options.Audience = Configuration["Authentication:oidc:ClientId"];
//                    options.RequireHttpsMetadata = false; // yolo, demo-time
//                    options.TokenValidationParameters = new TokenValidationParameters
//                    {
//                        ClockSkew = TimeSpan.FromMinutes(5),
//
//                        // how to update signing keys automatically?
//                        IssuerSigningKeys = discoveryDocument.SigningKeys,
//                        RequireSignedTokens = true,
//                        RequireExpirationTime = false, //Keycloak admin requests don't have an Expiration Time...
//                        ValidateLifetime = false, //Keycloak admin requests don't have an Lifetime...
//                        ValidateAudience = false, //Keycloak admin requests don't send an audience ...
//                        ValidAudience = Configuration["Authentication:oidc:ClientId"],
//                        ValidateIssuer = false, //Keycloak admin requests don't send an issuer ...
//                        ValidIssuer = Configuration["Authentication:oidc:Authority"],
//                    };
//                    options.Events = new JwtBearerEvents
//                    {
//                        OnAuthenticationFailed = ctxt => { return Task.CompletedTask; },
//                        OnTokenValidated = ctxt =>
//                        {
//                            var jwtSecurityToken = ctxt.SecurityToken as JwtSecurityToken;
//                            if (jwtSecurityToken != null)
//                            {
//                                var clientId = jwtSecurityToken.Claims.First(c => c.Type == "resource");
//                                var action = jwtSecurityToken.Claims.First(c => c.Type == "action");
//                                var notBefore = jwtSecurityToken.Claims.First(c => c.Type == "notBefore");
//                                var expiration = jwtSecurityToken.Claims.First(c => c.Type == "expiration");
//                                var tokenId = jwtSecurityToken.Claims.First(c => c.Type == "id");
//
//                                Console.WriteLine(tokenId);
//                            }
//
//                            // TODO handle Global Logout here or in AuthController.BackChannelLogout?
//                            // adapt logout logic from: org.keycloak.adapters.PreAuthActionsHandler#handleRequest
//
//                            return Task.CompletedTask;
//                        },
//                        OnChallenge = ctxt => { return Task.CompletedTask; },
//                        OnMessageReceived = async ctxt =>
//                        {
//                            if ("/keycloak/k_logout".Equals(ctxt.Request.Path))
//                            {
//                                using (StreamReader reader = new StreamReader(ctxt.Request.Body, Encoding.UTF8))
//                                {
//                                    // Keycloak sends the logout information as a signed JWT in the request body
//
//                                    // Token example for an "Empty" Logout Request
//                                    // eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJQcm1KRTB1eExWZkZlbWVPNDFwd3JaOFAzUHhtaThxNDRWNDU1M29sZWpFIn0.eyJpZCI6ImMwYTg2ZWRiLTBkZDAtNDQwOS05M2I4LWZlZjAzZWMxMDYzMC0xNTUxNzM0MDUzOTIxIiwiZXhwaXJhdGlvbiI6MTU1MTczNDA4MywicmVzb3VyY2UiOiJhcHAtZG5jLXdlYmFwcCIsImFjdGlvbiI6IkxPR09VVCIsIm5vdEJlZm9yZSI6MTU1MTczNDA1M30.Ts2ohgCcO5Ty8P4G5NDRJC_QFs9xJ8OTFUldX39dCATUUr9fdoHwsXOsdePTanA1g2RqeCd7qSFxXG6To4dnsL-2lYWIykI9zHpVmd0kD-XoqDEdZayhiH14yP2mawq4wE5lLGOXR5Gu2HV6dbc8ni1cWEfJ5owXb4y9KmWrTNx5LX05wBX_MvWps9CHAfYDWeKRmrfplW3OibbLJbMRaifHMVhg-Lu7WhrRGCHzB3FU4YSz9xWVQ0ESh4e8p6rr5vGHlteCZgPGCcK69tMoWSy_0li_jXmnp9K_edfY-YrGzoj1rIAoEKGgodbYPv2qFK3tWDQ5Bu6esfoM-4H-zg
//
//                                    // Token example for a Logout Request with 2 Sessions
//                                    // 
//
//                                    ctxt.Token = await reader.ReadToEndAsync();
//                                }
//                            }
//
//                            Console.WriteLine("ctxt.Token {0}", ctxt.Token);
//
//                            await Task.CompletedTask;
//                        }
//                    };
//                });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}