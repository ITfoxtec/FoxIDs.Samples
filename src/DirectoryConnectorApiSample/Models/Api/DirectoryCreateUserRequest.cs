namespace DirectoryConnectorApiSample.Models.Api;

public class DirectoryCreateUserRequest : DirectoryUserIdentifierRequest
{
    public string Password { get; set; }

    public bool ConfirmAccount { get; set; }

    public bool RequireMultiFactor { get; set; }

    public IEnumerable<ClaimValue> Claims { get; set; }
}
