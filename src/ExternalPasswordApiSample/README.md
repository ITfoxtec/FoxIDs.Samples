# ExternalPasswordApiSample

Sample implementation of the External Password API used by FoxIDs.

Endpoints:
- POST /validation
- POST /notification

Security:
- HTTP Basic authentication
  - Username: external_password
  - Password: (configured secret in appsettings.json: AppSettings:ApiSecret)

Request JSON (both endpoints):
```json
{
  "email": "user1@somewhere.org",
  "phone": "+4011223344",
  "username": "user1",
  "password": "MyPassword123!"
}
```
At least one of email / phone / username plus password is required.

Validation endpoint responses:
- 200 OK (accepted)
- 401 + {"error":"invalid_api_id_secret"}
- 400/403 + {"error":"password_not_accepted"}

Notification endpoint responses:
- 200 OK
- 401 + {"error":"invalid_api_id_secret"}

Run:
```
dotnet run
```
Open Swagger UI at:
```
https://localhost:44355/swagger
```
Use the Basic auth header: base64("external_password:YourSecret").

Update AppSettings:ApiSecret before using in production.
