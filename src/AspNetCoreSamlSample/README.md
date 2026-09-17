# ASP.NET Core SAML 2.0 sample

This sample signs in through FoxIDs using SAML 2.0. See the [sample setup guide](https://www.foxids.com/docs/samples) for the application registration and connection settings.

## Send login parameters

The optional example sends `profile_id` and a space-separated `departments` list as an additional query parameter alongside the SAML authentication request. It is **disabled by default**.

1. In FoxIDs Control, open this sample's **SAML 2.0 application registration**, enable **Show advanced**, and configure **Allowed request parameters**. Add `profile_id` and `departments`. FoxIDs automatically splits parameter values on spaces. FoxIDs must be configured to accept each parameter before it is made available to claim transforms.
2. In `appsettings.json`, set `Settings:SendLoginParameter` to `true` and replace `Settings:LoginParameterProfileId` with the ID to send:

   ```json
   "Settings": {
     "SendLoginParameter": true,
     "LoginParameterProfileId": "16b66fb6-9f88-4a10-b489-8e54f48f76a4",
     "LoginParameterDepartments": "Sales Support"
   }
   ```

   Merge these settings into the existing section. Keep the application, authentication method and other connection settings.
3. Start a new sign-in from the sample. `Login` in `Controllers/SamlController.cs` creates the SAML HTTP-Redirect binding, then appends `profile_id` and `departments` using `QueryHelpers.AddQueryString`. This preserves the encoded SAML message, RelayState and signature.
4. In FoxIDs, read `_local:params:profile_id` from the authentication method's first-level claim transforms. To retain an authorised value for later application logins, map it to an `_session:` claim.

The space-separated list automatically produces two `_local:params:departments` claims: `Sales` and `Support`. Keep the list in one parameter occurrence. Across all parameters, FoxIDs accepts up to 10 values, 500 characters per value and 1,000 characters combined.

Set `SendLoginParameter` back to `false` to stop sending the parameters. The configured values are sent on every SAML sign-in started while the example is enabled.

The additional query parameter is outside the signed SAML message. Treat it as browser input and verify the authenticated user's permissions and the target user's eligibility before using it for impersonation. Sending the parameter does not itself enable impersonation. Avoid sending secrets in query parameters.
