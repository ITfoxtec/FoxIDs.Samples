# DirectoryConnectorApiSample

Sample implementation of the Directory Connector API used by FoxIDs to validate passwords and delegate password lifecycle operations to an external directory.

Endpoints:
- GET `/health`
- POST `/authentication`
- POST `/create-user`
- POST `/change-password`
- POST `/set-password`

Authentication:
- HTTP Basic
  - Username: `directory_connector`
  - Password: configured secret in `appsettings.json`: `AppSettings:ApiSecret`

Demo users:
- `user1@somewhere.org` / `user1` / `+4511223344`, password `testpass1`, directory user ID `dir-user-1`
- `user2@somewhere.org` / `user2` / `+4555667788`, password `testpass2`, directory user ID `dir-user-2`
- `rejected@somewhere.org` / `rejected`, password `testpass3`, directory user ID `dir-user-rejected`: login rejected with a localised UI message
- `rejected-no-message@somewhere.org` / `rejected-no-message`, password `testpass4`, directory user ID `dir-user-rejected-no-message`: login rejected without a UI message
- `disabled@somewhere.org` / `disabled`, password `disabledpass1`, directory user ID `dir-user-disabled`

Authentication request:
```json
{
  "email": "user1@somewhere.org",
  "password": "testpass1"
}
```

Change-password request:
```json
{
  "directoryUserId": "dir-user-1",
  "email": "user1@somewhere.org",
  "currentPassword": "testpass1",
  "newPassword": "newpass123"
}
```

Set-password request:
```json
{
  "directoryUserId": "dir-user-1",
  "email": "user1@somewhere.org",
  "password": "testpass1"
}
```

Success response (200):
```json
{
  "directoryUserId": "dir-user-1",
  "email": "user1@somewhere.org",
  "phone": "+4511223344",
  "username": "user1",
  "confirmAccount": false,
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

Error responses:
- `401 invalid_api_id_secret`
- `401 user_not_exists`
- `401 invalid_password`
- `401 login_rejected` (only from `/authentication`, after verifying credentials)
- `401 invalid_current_password`
- `403 user_disabled`
- `400 password_min_length`
- `400 password_banned_characters`
- `400 new_password_equals_current`

## Login rejection and language

Authenticate the `rejected@somewhere.org` demo user with password `testpass3` to receive HTTP 401:

```json
{
  "error": "login_rejected",
  "errorMessage": "Credentials verified; login rejected by demo directory policy.",
  "uiErrorMessage": "You cannot log in here. Contact support."
}
```

FoxIDs displays `uiErrorMessage` as plain text on the login form. `errorMessage` is diagnostic text for the logs and is never the fallback user message.

FoxIDs sends its selected culture in `Accept-Language`. This sample supports English (`en`, including `en-US`) and Danish (`da`, including `da-DK`). Send `Accept-Language: da-DK` to receive `Du kan ikke logge ind her. Kontakt support.` Missing or unsupported language preferences fall back to English. Only the header selects the language; query-string and cookie culture providers are disabled.

Authenticate `rejected-no-message@somewhere.org` with password `testpass4` to receive `login_rejected` without `uiErrorMessage`. FoxIDs then displays the same general, localised login message as for `invalid_password`, `user_not_exists`, `user_disabled` and `user_deleted`. FoxIDs also uses this fallback for a null, empty or whitespace-only UI message.

Both examples work with or without the matching `directoryUserId`. A rejected login does not disable or delete the demo user, change its password or return a success response.

The sample verifies credentials before checking `RejectLogin`. A wrong password for either demo user returns `invalid_password` without a UI message or the rejection reason. An unknown identifier returns `user_not_exists`; FoxIDs shows the same general login message. A successful lookup or knowledge of a directory user ID does not prove that the person logging in owns the account. Preserve this order in a real connector and keep observable failure behaviour consistent before credential verification.

The Postman folder **Authentication - login rejection** includes English, Danish, no-message, directory-ID and invalid-credential examples with response assertions. The demo directory is stored in memory; use your external directory's credential validation in a real connector.

Run:
```bash
dotnet run
```

Swagger UI:
```text
https://localhost:44362/swagger
```

Use the Postman collection `directory-connector-api.postman_collection.json` to test the endpoints.

Update `AppSettings:ApiSecret` before using in production.
