using System.ComponentModel.DataAnnotations;
using ITfoxtec.Identity.Util;

namespace FoxIDs.ControlApiSample.Models
{
    public class ApiSampleSettings
    {
        /// <summary>
        /// API Sample client id.
        /// </summary>
        [Required]
        public string ClientId { get; set; }
        /// <summary>
        /// Sample seed tool client secret.
        /// </summary>
        [Required]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Space delimited list of scopes.
        /// </summary>
        [Required]
        public string Scope { get; set; }

        /// <summary>
        /// FoxIDs endpoint.
        /// </summary>
        [Required]
        public string FoxIDsEndpoint { get; set; }
        /// <summary>
        /// Set to false if you have configured a custom domain.
        /// </summary>
        public bool IncludeTenantInUrl { get; set; }
        /// <summary>
        /// Sample seed tool tenant.
        /// </summary>
        [Required]
        public string Tenant { get; set; }
        /// <summary>
        /// Sample seed tool track.
        /// </summary>
        [Required]
        public string Track { get; set; }
        /// <summary>
        /// FoxIDs tenant/track/application authority.
        /// </summary>
        public string Authority => UrlCombine.Combine(FoxIDsEndpoint, Tenant, "master", ClientId);

        /// <summary>
        /// FoxIDs API endpoint.
        /// </summary>
        [Required]
        public string FoxIDsConsolApiEndpoint { get; set; }
    }
}