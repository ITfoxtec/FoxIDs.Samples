using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetCoreSamlIdPSample.Models
{
    public class IdPInitiatedViewModel
    {
        [Display(Name = "Authentication method")]
        [Required]
        [MaxLength(500)]
        public string RelyingPartyIssuer { get; set; }

        public IEnumerable<SelectListItem> RelyingPartyIssuers { get; set; }

        [Display(Name = "Application (technical name)")]
        [Required]
        [MaxLength(500)]
        public string ApplicationName { get; set; }

        [Display(Name = "Application redirect URL - required for OpenID Connect (oidc)")]
        [MaxLength(500)]
        public string ApplicationRedirectURL { get; set; }

        [Display(Name = "Application type ('oidc' or 'saml2')")]
        [Required]
        [MaxLength(10)]
        public string ApplicationType { get; set; } 
    }
}
