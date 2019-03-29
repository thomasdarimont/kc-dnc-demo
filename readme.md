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

dotnet build
```

## Setup https for dotnet core
TODO

# Running

> Start Keycloak with the dnc-demo Realm 
```
docker run \
  -d \
  --name keycloak-dnc \
  -e KEYCLOAK_USER=admin \
  -e KEYCLOAK_PASSWORD=admin \
  --net=host \
  -p 8080:8080 \
  -v `pwd`/dnc-demo-realm.json:/config/dnc-demo-realm.json \
  -it jboss/keycloak:5.0.0 \
  -b 0.0.0.0 \
  -Djboss.http.port=8080 \
  -Dkeycloak.migration.action=import \
  -Dkeycloak.migration.provider=singleFile \
  -Dkeycloak.migration.file=/config/dnc-demo-realm.json \
  -Dkeycloak.migration.strategy=OVERWRITE_EXISTING
```

> Start the WebApp 
```
//TODO
```

> Start the WebAPI
```
//TODO
```

> Login via https://localhost:5001

* Login as user with tester:test
* Login as admin with arno:test


# Third-Party Components

The example uses the following third-party components:
* [Automatic Token Management](https://github.com/IdentityServer/IdentityServer4.Samples/tree/master/Clients/src/MvcHybridAutomaticRefresh/AutomaticTokenManagement) 
Automatically renews the Access-Token in the background of the WebApp module.
