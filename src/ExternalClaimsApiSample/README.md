# ExternalClaimsApiSample

Sample implementation of the [FoxIDs External Claims API](https://www.foxids.com/docs/claim-transform-task#external-claims---api), used to enrich a user's claims.

## Run and configure

Run from this directory:

```bash
dotnet run --no-launch-profile -- --urls https://localhost:44353
```

Open Swagger UI at `https://localhost:44353/swagger`. The endpoint is POST `/ExternalClaims/Claims`. Configure the External Claims transform in FoxIDs with the reachable base URL ending in `/ExternalClaims` and the secret from `AppSettings:ApiSecret`.

Requests require HTTP Basic authentication with username `external_claims` and that secret. Use a private secret and HTTPS for a deployed API.

## Claims format

Set `AppSettings:ClaimsFormat` to `ClaimsList` (the default) or `Properties`, and select the same **Claims format** on the External Claims transform in FoxIDs. The setting controls both requests and responses.

To use properties, set the following alongside the API secret in `appsettings.json`:

```json
"AppSettings": {
  "ApiSecret": "YourSecret",
  "ClaimsFormat": "Properties"
}
```

Restart the sample after changing the format. Swagger displays the configured request and response schemas. You can also override the setting when starting the sample:

```bash
dotnet run --no-launch-profile -- --urls https://localhost:44353 --AppSettings:ClaimsFormat=Properties
```

### Claims list

Request:

```json
{
  "claims": [
    { "type": "sub", "value": "somewhere/user1" },
    { "type": "email", "value": "user1@somewhere.org" }
  ]
}
```

Success response (HTTP 200):

```json
{
  "claims": [
    { "type": "sub", "value": "somewhere/external-user1@somewhere.org" },
    { "type": "role", "value": "admin_access" },
    { "type": "role", "value": "read_access" },
    { "type": "role", "value": "write_access" }
  ]
}
```

### Properties

Claims are properties directly at the JSON root.

Request:

```json
{
  "sub": "somewhere/user1",
  "email": "user1@somewhere.org"
}
```

Success response (HTTP 200):

```json
{
  "sub": "somewhere/external-user1@somewhere.org",
  "role": ["admin_access", "read_access", "write_access"]
}
```

A string represents one value; an array of strings represents multiple values for the same claim. Names are case-sensitive, and names and string contents are preserved. Duplicate property names, numbers, booleans, null claim values, nested objects and non-string array elements are rejected. Both formats apply the same required-name and required-value validation.

An empty list or an empty properties object is accepted. An empty array contributes no values. The demo uses the first `email` value, falling back to `sub` and then `unknown`, to construct its returned subject. It always returns the three demo roles.

## Errors

Invalid API credentials return HTTP 401 in either format:

```json
{ "error": "invalid_api_id_secret", "errorMessage": "Invalid API ID or secret." }
```

Malformed requests and requests in the wrong configured format return HTTP 400 model-validation errors.

## Test both formats

Import `external-claims-api.postman_collection.json` into Postman. Set its `baseUrl`, `apiSecret` and `claimsFormat` variables. The collection generates the request in the selected format and checks the returned subject and all three roles. Its `claimsFormat` must match the running sample; it does not change the server configuration.

Run once with `ClaimsList`, then restart the sample with `Properties`, change the collection variable and run again. For an end-to-end test with FoxIDs, change the transform's **Claims format** to match each run.

Run the HTTP contract tests from the repository root:

```bash
dotnet test test/ExternalClaimsApiSample.Tests/ExternalClaimsApiSample.Tests.csproj
```
