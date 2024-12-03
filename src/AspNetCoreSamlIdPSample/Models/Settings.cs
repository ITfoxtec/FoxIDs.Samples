using FoxIDs.SampleHelperLibrary.Models;
using System.Collections.Generic;

namespace AspNetCoreSamlIdPSample.Models
{
    public class Settings : LibrarySettings
    {
        public List<RelyingParty> RelyingParties { get; set; }    
    }
}
