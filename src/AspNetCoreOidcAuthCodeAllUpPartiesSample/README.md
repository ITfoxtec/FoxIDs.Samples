# ASP.NET Core OpenID Connect sample with all authentication methods

This sample signs in through FoxIDs using the OpenID Connect authorisation code flow. See the [sample setup guide](https://www.foxids.com/docs/samples) for the application registration and connection settings.

The sample can send ordinary login parameters and an impersonation parameter independently. Both examples are **disabled by default**. Merge the settings below into the existing `IdentitySettings` section in `appsettings.json`, keeping the authority, client and other connection settings. Start or restart the sample after changing these settings.

## Send login parameters

This example sends `profile_id` and a space-separated `departments` list with the OIDC authorisation request.

1. In FoxIDs Control, open this sample's **OpenID Connect application registration**, enable **Show advanced**, and add `profile_id` and `departments` to **Allowed request parameters**. Each parameter must be configured in FoxIDs before it is available to claim transforms.
2. Set `IdentitySettings:SendLoginParameters` to `true` and configure the values to send:

   ```json
   "IdentitySettings": {
     "SendLoginParameters": true,
     "LoginParameterProfileId": "16b66fb6-9f88-4a10-b489-8e54f48f76a4",
     "LoginParameterDepartments": "Sales Support"
   }
   ```

3. Start a new sign-in from the sample. `OnRedirectToIdentityProvider` in `Program.cs` adds the parameters using `ProtocolMessage.SetParameter`.
4. In FoxIDs, read `_local:params:profile_id` and `_local:params:departments` from the authentication method's first-level claim transforms.

FoxIDs automatically splits values on spaces, so `departments` produces two claims: `Sales` and `Support`. Keep the list in one parameter occurrence. Across all parameters, FoxIDs accepts up to 10 values, 500 characters per value and 1,000 characters combined.

Set `SendLoginParameters` to `false` to stop sending these parameters. The configured values are sent on every OIDC sign-in while this setting is enabled.

## Send an impersonation parameter

This example sends `impersonated_user_id` as a reference to the user to impersonate. The reference can be an external profile ID or an internal user ID used by the configured target lookup.

1. Add `impersonated_user_id` to **Allowed request parameters** in the sample's FoxIDs application registration.
2. Enable the parameter and set the target user's reference:

   ```json
   "IdentitySettings": {
     "SendImpersonationParameter": true,
     "ImpersonatedUserId": "16b66fb6-9f88-4a10-b489-8e54f48f76a4"
   }
   ```

3. Configure the FoxIDs authentication method's claim transforms to read `_local:params:impersonated_user_id`, check the authenticated user's permission to impersonate, load the target using `Query internal user` or a Claims API, check the target's eligibility, and select it with an `Impersonation` transform. Follow the [impersonation configuration example](https://www.foxids.com/docs/claim-transform-task#impersonation).
4. To continue impersonation at later application logins, save the accepted target reference in `_session:impersonated_user_id`. When the request parameter is absent, load the target using that saved reference and repeat the permission checks.
5. Start a new sign-in from the sample. The configured reference is sent on every OIDC sign-in while `SendImpersonationParameter` is enabled, independently of `SendLoginParameters`.

To select another target, change `ImpersonatedUserId` and start a new sign-in. Setting `SendImpersonationParameter` to `false` stops sending the parameter. If FoxIDs retains the target reference in a session claim, the configured transforms can continue impersonation. To end it, the FoxIDs flow must clear the saved reference and complete without selecting a target.

Treat all parameters as browser input. Sending a target reference does not grant permission to impersonate. Avoid sending secrets in query parameters.
