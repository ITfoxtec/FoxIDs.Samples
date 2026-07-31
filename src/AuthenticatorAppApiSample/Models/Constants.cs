namespace AuthenticatorAppApiSample.Models;

public static class Constants
{
    public const string BasicAuthApiId = "authenticator_app";

    public static class NotificationTypes
    {
        public const string Registered = "registered";
    }

    public static class Errors
    {
        public const string InvalidApiIdOrSecret = "invalid_api_id_secret";
        public const string InvalidRequest = "invalid_request";
    }
}
