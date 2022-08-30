using System.ComponentModel.DataAnnotations;
using UrlCombineLib;

namespace FoxIDs.SampleSeedTool.Models
{
    public class SeedSettings
    {
        /// <summary>
        /// Sample seed tool client id.
        /// </summary>
        public string ClientId => DownParty;
        /// <summary>
        /// Sample seed tool client secret.
        /// </summary>
        [Required]
        public string ClientSecret { get; set; }

        /// <summary>
        /// FoxIDs endpoint.
        /// </summary>
        [Required]
        public string FoxIDsEndpoint { get; set; }
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
        /// Sample seed tool down party (client id).
        /// </summary>
        [Required]
        public string DownParty { get; set; }
        /// <summary>
        /// FoxIDs tenant/track/downparty authority.
        /// </summary>
        public string Authority => UrlCombine.Combine(FoxIDsEndpoint, Tenant, "master", DownParty);

        /// <summary>
        /// FoxIDs API endpoint.
        /// </summary>
        [Required]
        public string FoxIDsConsolApiEndpoint { get; set; }
    }
}