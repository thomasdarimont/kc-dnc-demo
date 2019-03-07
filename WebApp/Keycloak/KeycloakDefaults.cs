namespace WebApp.Keycloak
{
    public class KeycloakDefaults
    {
        /// <summary>
        /// The default value used for KeycloakOptions.AuthenticationScheme.
        /// </summary>
        public const string AuthenticationScheme = "Keycloak";

        public const string CallbackPath = "/oauth/callback";

        public const string SignedOutCallbackPath = "/oauth/logout";

        public const string SignedOutRedirectUri = "/";
    }
}