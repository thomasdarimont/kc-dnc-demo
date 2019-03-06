using System;

namespace IdentityModel.AspNetCore
{
    public class AutomaticTokenManagementOptions
    {
        public string Scheme { get; set; } = "Keycloak";
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
        public bool RevokeRefreshTokenOnSignout { get; set; } = false; //keycloak doesn't suppport that yet... d'uh - see https://issues.jboss.org/browse/KEYCLOAK-5325
    }
}