# DirectoryConnectorApiSample

Sample implementation of the [FoxIDs Directory Connector API](https://www.foxids.com/docs/directory-connector), including on-demand user synchronisation and password operations in an external directory.

The sample uses an in-memory directory with plaintext demo passwords. Restarting resets all users and passwords. Replace `DemoDirectoryStore` with your external directory integration for real use.

## Run and configure

Run from this directory:

```bash
dotnet run --no-launch-profile -- --urls https://localhost:44362
```

Open Swagger UI at `https://localhost:44362/swagger`. Configure FoxIDs with the sample's reachable base URL and the secret from `AppSettings:ApiSecret`. All five connector endpoints require HTTP Basic authentication with username `directory_connector` and that secret. Use a private secret and HTTPS for a deployed connector. The separate GET `/health` endpoint is a demonstration health check.

Import `directory-connector-api.postman_collection.json` into Postman. Its variables contain the local base URL and demo API credentials. Set the collection's `claimsFormat` variable to match `AppSettings:ClaimsFormat`; the collection generates create-user claims and checks synchronisation responses in that format. Run the collection in order against a freshly started sample; it includes response assertions and password changes. Restart the sample before running it again.

## Claims format

Set `AppSettings:ClaimsFormat` to `ClaimsList` (the default) or `Properties`, and select the same **Claims format** on the Directory Connector in FoxIDs environment settings.

```json
"AppSettings": {
  "ApiSecret": "YourSecret",
  "ClaimsFormat": "Properties"
}
```

The format applies to the `claims` member in create-user requests and synchronisation responses. Identifiers, passwords and account settings remain ordinary fields at the JSON root. The other endpoint contracts are the same in both formats.

Restart the sample after changing the format. Swagger displays the configured schemas. You can also start with an override:

```bash
dotnet run --no-launch-profile -- --urls https://localhost:44362 --AppSettings:ClaimsFormat=Properties
```

### Claims list

The endpoint examples below use `ClaimsList`. Multiple values use separate list entries with the same type:

```json
"claims": [
  { "type": "name", "value": "User Two" },
  { "type": "role", "value": "read_access" },
  { "type": "role", "value": "write_access" }
]
```

### Properties

A create-user request in `Properties` format:

```json
{
  "email": "newuser@somewhere.org",
  "password": "testpass123",
  "confirmAccount": false,
  "requireMultiFactor": false,
  "claims": {
    "name": "New User",
    "role": ["read_access", "write_access"]
  }
}
```

After creation, synchronising by the returned directory ID returns the full account snapshot with this `claims` member:

```json
"claims": {
  "name": "New User",
  "role": ["read_access", "write_access"]
}
```

A string represents one value; an array of strings represents multiple values for the same claim. Names are case-sensitive, and names and string contents are preserved. Duplicate claim property names, numbers, booleans, null claim values, nested objects and non-string array elements are rejected. Both formats apply the same required-name and required-value validation.

The optional `claims` member can be omitted or null when creating a user. An empty list, an empty properties object or an empty value array also adds no claims. Synchronisation returns an empty list or object in the configured format. Invalid claims or a mismatched format return HTTP 400 before an account is created.

## Demo users

| Directory user ID | Identifier | Password | Behaviour |
| --- | --- | --- | --- |
| `dir-user-1` | `user1@somewhere.org`, `user1`, `+4511223344` | `testpass1` | Normal login |
| `dir-user-2` | `user2@somewhere.org`, `user2`, `+4555667788` | `testpass2` | Normal login with additional roles |
| `dir-user-rejected` | `rejected@somewhere.org`, `rejected` | `testpass3` | Verified login rejected with a localised UI message |
| `dir-user-rejected-no-message` | `rejected-no-message@somewhere.org`, `rejected-no-message` | `testpass4` | Verified login rejected without a UI message |
| `dir-user-disabled` | `disabled@somewhere.org`, `disabled` | `disabledpass1` | Synchronisation returns `disableAccount: true`; password operations are rejected |
| `dir-user-deleted` | `deleted@somewhere.org`, `deleted` | `testpass5` | Deleted account |
| `dir-user-expired` | `expired@somewhere.org`, `expired` | `testpass6` | Password must be changed |
| `dir-user-setup-email` | `setup@somewhere.org` | None | Set a password using an email confirmation code |
| `dir-user-setup-sms` | `+4511223377` | None | Set a password using an SMS confirmation code |

For first-login password setup, enable the corresponding email or SMS flow and notification delivery in FoxIDs. FoxIDs verifies the confirmation code before calling `set-password`. This also supports users who have forgotten their password before their first sign-in.

## Synchronisation request

POST `/synchronise-user` with exactly one current identifier (`email`, `phone` or `username`) for the initial lookup:

```json
{
  "email": "user1@somewhere.org"
}
```

For an account already known to FoxIDs, send its stable directory ID:

```json
{
  "directoryUserId": "dir-user-1"
}
```

When supplied, the directory ID determines the account; the sample does not fall back to an identifier. The successful response is HTTP 200 with the current user snapshot. Only this endpoint returns identifiers, account state and claims:

```json
{
  "directoryUserId": "dir-user-1",
  "email": "user1@somewhere.org",
  "phone": "+4511223344",
  "username": "user1",
  "confirmAccount": false,
  "disableAccount": false,
  "changePassword": false,
  "setPasswordEmail": false,
  "setPasswordSms": false,
  "disableSetPasswordEmail": false,
  "disableSetPasswordSms": false,
  "passwordLastChanged": null,
  "emailVerified": true,
  "phoneVerified": true,
  "disableTwoFactorApp": false,
  "disableTwoFactorSms": false,
  "disableTwoFactorEmail": false,
  "requireMultiFactor": false,
  "claims": [
    { "type": "name", "value": "User One" },
    { "type": "role", "value": "read_access" }
  ]
}
```

`passwordLastChanged` is the password-change time in Unix seconds, or null when unknown. Password changes and user creation set it in this sample. A disabled user still returns a successful snapshot with `disableAccount: true`. An identifier lookup that finds no active directory entry returns the error `user_not_exists`; a lookup by a known directory ID that no longer exists returns the error `user_deleted`.

## Authentication request

POST `/authentication`:

```json
{
  "directoryUserId": "dir-user-1",
  "password": "testpass1"
}
```

Success: HTTP 204 with no body. The sample verifies the password before returning login rejection guidance, password expiry or password-policy errors.

## Create-user request

POST `/create-user` with exactly one identifier:

```json
{
  "email": "newuser@somewhere.org",
  "password": "testpass123",
  "confirmAccount": false,
  "requireMultiFactor": false,
  "claims": [
    { "type": "name", "value": "New User" }
  ]
}
```

Success: HTTP 200 with only the new directory ID. The value below is illustrative; the sample generates a new ID for each account:

```json
{
  "directoryUserId": "dir-user-8aecb4d74c744125a45e98f3ec1e2b70"
}
```

FoxIDs then calls `synchronise-user` with that ID to retrieve the account.

## Change-password request

POST `/change-password`:

```json
{
  "directoryUserId": "dir-user-1",
  "currentPassword": "testpass1",
  "newPassword": "newpass123"
}
```

Success: HTTP 204 with no body. The current password is verified before checking the new password. An expired current password can be changed. Success clears the change-password and initial password-setup requirements.

## Set-password request

POST `/set-password`:

```json
{
  "directoryUserId": "dir-user-setup-email",
  "password": "newpass123"
}
```

Success: HTTP 204 with no body. This operation trusts the authorised FoxIDs API caller to have verified the user's recovery or setup code; the connector does not validate a current password. Success clears the change-password and initial password-setup requirements.

## Errors

The sample returns the following errors for valid connector requests:

| Endpoint | HTTP status and error |
| --- | --- |
| All five endpoints | `401 invalid_api_id_secret` |
| Synchronisation by identifier | `401 user_not_exists` |
| Synchronisation by directory ID | `403 user_deleted` |
| Authentication, change-password, set-password | `403 operation_rejected` for an unknown, deleted or disabled account |
| Authentication | `401 invalid_password`; after credential verification: `401 login_rejected` or `400 password_expired` |
| Create-user | `400 user_exists` |
| Change-password | `401 invalid_current_password`, `400 new_password_equals_current` |
| Authentication, create-user, change-password, set-password | `400 password_min_length`, `400 password_banned_characters` |

The demo password policy requires at least eight characters and rejects passwords containing `Forbidden!` or the account's username. It does not implement password history. Malformed requests, such as password operations missing `directoryUserId`, receive HTTP 400 model-validation errors.

`operation_rejected` reports a failed operation without changing the FoxIDs account. FoxIDs refreshes account state through synchronisation. Other supported codes and their permitted endpoints are described in the [Directory Connector error contract](https://www.foxids.com/docs/directory-connector#error-response).

### Login rejection and language

Authenticate `dir-user-rejected` with password `testpass3` to receive HTTP 401:

```json
{
  "error": "login_rejected",
  "errorMessage": "Credentials verified; login rejected by demo directory policy.",
  "uiErrorMessage": "You cannot log in here. Contact support."
}
```

FoxIDs displays `uiErrorMessage` as plain text on the login form. `errorMessage` is English diagnostic text for the logs and is never the fallback user message.

FoxIDs sends its selected culture in `Accept-Language`. This sample supports English (`en`, including `en-US`) and Danish (`da`, including `da-DK`). Send `Accept-Language: da-DK` to receive `Du kan ikke logge ind her. Kontakt support.` Missing or unsupported language preferences fall back to English. Only the header selects the language; query-string and cookie culture providers are disabled.

Authenticate `dir-user-rejected-no-message` with password `testpass4` to receive `login_rejected` without `uiErrorMessage`. FoxIDs then uses its general, localised login error. A wrong password for either user returns `invalid_password` without a UI message or the policy rejection reason. Keep credential verification before account-specific guidance in a real connector. API authentication or knowledge of a directory user ID does not establish that the person logging in owns the account.

## Tests

For an end-to-end test, run the sample and the Postman collection with `ClaimsList`, then restart with `Properties` and change the collection's `claimsFormat` variable. Set the same format in FoxIDs when testing through a Login authentication method. The Postman setting only controls the test requests and assertions; it does not change the server configuration.

The collection checks multiple roles and creates a user whose claims are retrieved by the following synchronisation request.

Run the HTTP contract tests from the repository root:

```bash
dotnet test test/DirectoryConnectorApiSample.Tests/DirectoryConnectorApiSample.Tests.csproj
```
