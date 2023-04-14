using System.Collections.Generic;
using System.Linq;
using ITfoxtec.Identity.Util;

namespace FoxIDs.SampleHelperLibrary.Models
{
    public class FoxIDsSettings
    {
        public string Authority => FoxIDsEndpoint == null ? null : UrlCombine.Combine(FoxIDsEndpoint, GetAuthorityElements(true).ToArray());
        public string AuthorityWithoutUpParty => FoxIDsEndpoint == null ? null : UrlCombine.Combine(FoxIDsEndpoint, GetAuthorityElements(false).ToArray());

        public string FoxIDsEndpoint { get; set; }
        public bool IncludeTenantInUrl { get; set; }
        public string Tenant { get; set; }
        public string Track { get; set; }
        public string DownParty { get; set; }
        public string UpParty { get; set; }

        private IEnumerable<string> GetAuthorityElements(bool includeUpParty)
        {
            if (IncludeTenantInUrl)
            {
                yield return Tenant;
            }
            yield return Track;
            if (includeUpParty)
            {
                yield return $"{DownParty}({UpParty})";
            }
            else
            {
                yield return DownParty;
            }
        }
    }
}
