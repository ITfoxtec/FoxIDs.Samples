namespace ExternalExtendedUiApiSample.Models.Api
{
    public class ErrorResponse
    {
        public string Error { get; set; }
        public string ErrorMessage { get; set; }

        public string UiErrorMessage { get; set; }

        public IEnumerable<ElementError> Elements { get; set; }
    }
}
