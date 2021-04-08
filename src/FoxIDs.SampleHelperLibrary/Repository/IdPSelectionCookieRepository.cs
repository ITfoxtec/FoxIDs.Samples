using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FoxIDs.SampleHelperLibrary.Repository
{
    public class IdPSelectionCookieRepository : BaseCookieRepository
    {

        public IdPSelectionCookieRepository(IDataProtectionProvider dataProtection, IHttpContextAccessor httpContextAccessor) : base(dataProtection, httpContextAccessor)
        { }

        public Task<string> GetAsync()
        {
            return GetValueAsync();
        }

        public Task SaveAsync(string idp)
        {
            return SaveValueAsync(idp);
        }

        public Task DeleteAsync()
        {
            return DeleteValueAsync();
        }

        protected override string PostCookieName => "selected.idp";
    }
}
