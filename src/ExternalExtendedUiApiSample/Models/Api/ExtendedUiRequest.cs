namespace ExternalExtendedUiApiSample.Models.Api
{
    public class ExtendedUiRequest
    {
        public IEnumerable<ElementValue> Elements { get; set; }

        public IEnumerable<ClaimValue> Claims { get; set; }
    }
}
