namespace ITfoxtec.Identity.Models
{
    /// <summary>
    /// Scope and roles authorization model.
    /// </summary>
    public class IdPWithScopes
    {
        /// <summary>
        /// The IdP value to match.
        /// </summary>
        public string IdP { get; set; }

        /// <summary>
        /// The list of scope values to match one or more of. A scope is not required if the list is null or empty.
        /// </summary>
        public IEnumerable<string> Scopes { get; set; }
    }
}
