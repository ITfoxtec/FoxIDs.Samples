# ExternalExtendedUiApiSample

Sample implementation of the External Extended UI API used by FoxIDs to perform custom validation of dynamic UI elements and optionally return additional claims.

Endpoint:
- POST /ExtendedUi/Validate

Authentication:
- HTTP Basic
  - Username: external_extended_ui
  - Password: (configured secret in appsettings.json: AppSettings:ApiSecret)

Request (example with one dynamic element):
```json
{
  "claims": [
    { "type": "sub", "value": "somewhere/user1" }
  ],
  "elements": [
    { "name": "ktvywqwc", "type": "text", "value": "123456" }
  ]
}
```

Success response (200):
```json
{
  "claims": [
    { "type": "my_custom_info_claim", "value": "some information" },
    { "type": "my_custom_field_claim", "value": "text-Element-123456" }
  ]
}
```

Validation error examples (400):
```json
{ "error": "invalid", "errorMessage": "Invalid value '111' in element 'ktvywqwc'." }
```

Field level error (element specific):
```json
{
  "error": "invalid",
  "elements": [
    { "name": "ktvywqwc", "uiErrorMessage": "Please use another value." }
  ]
}
```

Unauthorized (401):
```json
{ "error": "invalid_api_id_secret", "errorMessage": "Invalid API ID or secret." }
```

Run:
```
dotnet run
```
Swagger UI:
```
https://localhost:44354/swagger
```
Use the Basic auth header: base64("external_extended_ui:YourSecret").

Update AppSettings:ApiSecret before using in production.
