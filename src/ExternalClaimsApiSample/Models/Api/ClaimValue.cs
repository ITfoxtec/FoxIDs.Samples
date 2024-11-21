﻿using System.ComponentModel.DataAnnotations;

namespace ExternalClaimsApiSample.Models.Api
{
    public class ClaimValue
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
