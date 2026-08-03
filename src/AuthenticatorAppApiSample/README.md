# AuthenticatorAppApiSample

Sample implementation of the API notified by FoxIDs after a user registers a new authenticator app.

Endpoint:

- `POST /notification`

Security:

- HTTP Basic authentication
  - Username: `authenticator_app`
  - Password: the secret configured in `AppSettings:ApiSecret`

Request:

```json
{
  "type": "registered",
  "registration_id": "7a772286-76a2-4f17-a0f8-4e927bb1772d",
  "user_id": "e061ed17-7b44-48a8-b224-ecdb800ed5cc",
  "email": "user@example.com",
  "phone": "+4512345678",
  "username": "user@example.com"
}
```

The API returns `200 OK` when the required synchronous work is complete. FoxIDs treats every other response as a failed notification, restores the previous authenticator app registration and stops the login.

Run:

```text
dotnet run
```

Open Swagger UI at:

```text
https://localhost:44357/swagger
```

Configure `https://localhost:44357` as the base API URL in the Login authentication method. FoxIDs appends `/notification` and calls `https://localhost:44357/notification`.

For example, if the sample is deployed at `https://api.example.com/authenticator-app`, configure `https://api.example.com/authenticator-app` as the base API URL in FoxIDs. FoxIDs then calls `https://api.example.com/authenticator-app/notification`. Do not include `/notification` in the configured base API URL.

Update `AppSettings:ApiSecret` before using the sample outside local development.
