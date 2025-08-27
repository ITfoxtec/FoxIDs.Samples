# ExternalClaimsApiSample

Sample implementation of the External Claims API used by FoxIDs to enrich a user’s claims.

Endpoint:
- POST /ExternalClaims/Claims

Authentication:
- HTTP Basic
  - Username: external_claims
  - Password: (configured secret in appsettings.json: AppSettings:ApiSecret)

Request (FoxIDs sends existing claims; you may add more):
```json
{
  "claims": [
    { "type": "sub", "value": "somewhere/user1" },
    { "type": "email", "value": "user1@somewhere.org" }
  ]
}
```

Success response (200):
```json
{
  "claims": [
    { "type": "sub", "value": "somewhere/external-user1" },
    { "type": "role", "value": "read_access" },
    { "type": "role", "value": "write_access" }
  ]
}
```

Error response (401 invalid basic auth):
```json
{ "error": "invalid_api_id_secret", "errorMessage": "Invalid API ID or secret." }
```

Run:
```
dotnet run
```
Swagger UI:
```
https://localhost:44353/swagger
```
Use the Basic auth header: base64("external_claims:YourSecret").

Update AppSettings:ApiSecret before using in production.
