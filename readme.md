# PoC for ASP.Net Core Keycloak Integration
Simple example for securing an AspNetCore Web App with Keycloak.

The example consists of two modules:
* WebApi - A simple stateless REST Web-Service that is secured with JWT authentication.
* WebApp - A simple Web App that is secured via Keycloak.

The WebApp module demonstrates a basic integration with Keycloak by leveraging the built-in OpenID Connect support in AspNetCore which is additionally augmented with Keycloak specific configuration, like client role extraction. Further more, the WebApp calls the WebApi with an Access-Token provided after a successful authentication, to demonstrate calls to backend services.

The following features are currently supported:
* Single-Sign in with Keycloak
* Logout with Keycloak
* Access client specific role information (`resource_access` claim)
* Automatic Access-Token refresh in background
* Extract Access-Token to call backend-services.

# Building
Note you need .Net Core 2.2, which you can get here: [.Net Core 2.2 Download](https://dotnet.microsoft.com/download/dotnet-core/2.2)

```
dotnet restore
```

# Running

> Import `dnc-demo-realm.json` via:
```
bin/standalone.sh \
-Djboss.socket.binding.port-offset=10000 \
-Dkeycloak.migration.action=import \
-Dkeycloak.migration.file=/path/to/dnc-demo-realm.json \
-Dkeycloak.migration.strategy=OVERWRITE_EXISTING
```

> Start keycloak
```
bin/standalone.sh
```

> Start the WebApp and WebApi and login via https://localhost:5001
* Login with tester:test
* Login with admin:test


# Third-Party Components

The example uses the following third-party components:
* [Automatic Token Management](https://github.com/IdentityServer/IdentityServer4.Samples/tree/master/Clients/src/MvcHybridAutomaticRefresh/AutomaticTokenManagement) 
Automatically renews the Access-Token in the background of the WebApp module.
