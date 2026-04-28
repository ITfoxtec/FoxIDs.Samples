# DirectoryConnectorApiSample

Sample implementation of the Directory Connector API used by FoxIDs to validate passwords and delegate password lifecycle operations to an external directory.

Endpoints:
- POST `/DirectoryConnector/authentication`
- POST `/DirectoryConnector/change-password`
- POST `/DirectoryConnector/set-password`

Authentication:
- HTTP Basic
  - Username: `directory_connector`
  - Password: configured secret in `appsettings.json`: `AppSettings:ApiSecret`

Demo users:
- `user1@somewhere.org` / `user1` / `+4511223344`, password `testpass1`, directory user ID `dir-user-1`
- `user2@somewhere.org` / `user2` / `+4555667788`, password `testpass2`, directory user ID `dir-user-2`
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
  "disableAccount": false,
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
- `401 invalid_username_password`
- `401 invalid_current_password`
- `403 user_disabled`
- `400 password_min_length`
- `400 password_banned_characters`
- `400 new_password_equals_current`

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
