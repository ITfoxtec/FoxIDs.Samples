namespace BlazorBFFAspNetOidcSample
{
    public static class Constants
    {
        public const string AntiforgeryTokenHeaderName = "X-CSRF-TOKEN";

        public static class Client
        {
            public const string HttpClientLogicalName = "server";
            public const string HttpClientSecureLogicalName = "server.secure";
        }
    }
}
