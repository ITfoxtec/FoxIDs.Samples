# ExternalLoginApiSample

Sample implementation of the External Login API used by FoxIDs to validate a username/password pair against an existing user store.

Endpoint:
- POST /ExternalLogin/Authentication

Authentication:
- HTTP Basic
  - Username: external_login
  - Password: (configured secret in appsettings.json: AppSettings:ApiSecret)

Request (Text username example):
```json
{
  "usernameType": 200,
  "username": "user1",
  "password": "testpass1"
}
```

Request (Email username example):
```json
{
  "usernameType": 100,
  "username": "user1@somewhere.org",
  "password": "testpass1"
}
```

UsernameType codes:
- email = 100
- text = 200

Success response (200):
```json
{
  "claims": [
    { "type": "sub", "value": "somewhere/user1" },
    { "type": "email", "value": "user1@somewhere.org" },
    { "type": "role", "value": "read_access" }
  ]
}
```

Error responses:
- 401 invalid API ID or secret
```json
{ "error": "invalid_api_id_secret", "errorMessage": "Invalid API ID or secret." }
```

- 400 / 401 / 403 invalid username/password
```json
{ "error": "invalid_username_password", "errorMessage": "Invalid username or password." }
```

Run:
```
dotnet run
```
Swagger UI:
```
https://localhost:44352/swagger
```
Use the Basic auth header: base64("external_login:YourSecret").

Update AppSettings:ApiSecret before using in production.
