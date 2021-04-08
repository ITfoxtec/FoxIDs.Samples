using FoxIDs.SampleHelperLibrary.Models;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FoxIDs.SampleHelperLibrary.Repository
{
    public class IdPSessionCookieRepository : BaseCookieRepository
    {

        public IdPSessionCookieRepository(IDataProtectionProvider dataProtection, IHttpContextAccessor httpContextAccessor) : base(dataProtection, httpContextAccessor)
        { }

        public async Task<IdPSession> GetAsync()
        {
            return (await GetValueAsync()).ToObject<IdPSession>();
        }

        public Task SaveAsync(IdPSession idPSession)
        {
            return SaveValueAsync(idPSession.ToJson());
        }

        public Task DeleteAsync()
        {
            return DeleteValueAsync();
        }

        protected override string PostCookieName => "idp.session";
    }
}
