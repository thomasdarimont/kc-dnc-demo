# PoC for ASP.Net Core Keycloak Integration
Simple example for securing an AspNetCore Web App with Keycloak.

The example consists of two modules:
* WebApi - A simple stateless REST Web-Service that is secured with JWT authentication.
* WebApp - A simple Web App that is secured via Keycloak.

The WebApp demonstrates a basic integration with Keycloak. The following features are currently supported:
* Single-Sign in with Keycloak
* Logout with Keycloak
* Access client specific role information (`resource_access` claim)
* Automatic Access-Token refresh in background
* Extract Access-Token to call backend-services.

# Third-Party Components

The 
[Automatic Token Management](https://github.com/IdentityServer/IdentityServer4.Samples/tree/master/Clients/src/MvcHybridAutomaticRefresh/AutomaticTokenManagement) automatically renews the Access-Token in the background.
