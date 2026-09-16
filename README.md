# FoxIDs.Samples
The repository contains Identity Provide (IdP) / OpenID Provider (OP) and application samples connected to FoxIDs showing [login](https://www.foxids.com/docs/login) and logout with [OpenID Connect](https://www.foxids.com/docs/oidc) and [SAML 2.0](https://www.foxids.com/docs/saml-2.0). In addition, the samples show API calls secured with [OAuth 2.0](https://www.foxids.com/docs/oauth-2.0).

The samples are pre-configured and can be run immediately in Visual Studio.

> FoxIDs [documentation](https://www.foxids.com/docs) and a guide on how to use the [samples](https://www.foxids.com/docs/samples) in FoxIDs.

> FoxIDs GitHub repository [https://github.com/ITfoxtec/FoxIDs](https://github.com/ITfoxtec/FoxIDs)

## Small authentication cookies in the application samples

The ASP.NET Core OIDC, SAML and WS-Federation applications, Blazor Server and Blazor BFF server use `AddTestInMemoryTicketStore` from `FoxIDs.SampleHelperLibrary`. Claims, authentication properties and saved tokens are stored on the server through ASP.NET Core's [cookie authentication session store](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.cookies.cookieauthenticationoptions.sessionstore). The browser receives only a protected session reference. This reduces the combined cookie header size when testing several applications on the same host, while keeping tokens available for API calls, token renewal and logout.

**Test implementation only:** `TestInMemoryTicketStore` stores tickets in the application's memory. Sessions are lost when the application restarts or entries are evicted, and the store is not shared between application instances. Tickets expire with the authentication session, are updated on renewal and are removed on logout.

For production, replace this registration with an `ITicketStore` backed by shared storage or a database (for example Redis or SQL). Preserve ticket expiration, renewal and deletion, protect the stored claims and tokens, and isolate each application's sessions. All instances of the same application also need a shared ASP.NET Core Data Protection key ring and application name to read its protected session cookies.

After switching to server-side tickets, sign in again. If old large cookies already cause a "request headers too large" error, clear the cookies for the sample host first, including any old cookie chunks.

The bearer-token APIs and standalone Blazor WebAssembly application do not use ASP.NET Core authentication cookies and do not need this store.

Run the session store tests with `dotnet test test/FoxIDs.SampleHelperLibrary.Tests/FoxIDs.SampleHelperLibrary.Tests.csproj` (.NET 10 SDK). They verify small cookies with large claims and tokens, SAML's cookie scheme, token renewal, expiration and logout.
