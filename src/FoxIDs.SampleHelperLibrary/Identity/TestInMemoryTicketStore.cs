using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace FoxIDs.SampleHelperLibrary.Identity
{
    /// <summary>
    /// Stores authentication tickets on the server so the browser only needs a small session cookie.
    /// TEST ONLY: Tickets are held in this process and are lost on restart or cache eviction.
    /// In production, replace this with an ITicketStore backed by shared storage or a database.
    /// </summary>
    public class TestInMemoryTicketStore : ITicketStore
    {
        private const string KeyPrefix = "FoxIDs.Samples.AuthenticationTicket:";
        private readonly IMemoryCache cache;

        public TestInMemoryTicketStore(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = KeyPrefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            await RenewAsync(key, ticket);
            return key;
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            // The cookie middleware sets ExpiresUtc before storing or renewing a ticket.
            var expiresUtc = ticket.Properties.ExpiresUtc
                ?? throw new InvalidOperationException("The authentication ticket must have an expiration time.");

            // Serialize to give each request its own copy of the claims and saved tokens.
            // Changes (including refreshed tokens) are persisted only when the middleware renews the ticket.
            cache.Set(key, TicketSerializer.Default.Serialize(ticket), expiresUtc);
            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            var ticket = cache.TryGetValue(key, out byte[] value)
                ? TicketSerializer.Default.Deserialize(value)
                : null;
            return Task.FromResult(ticket);
        }

        public Task RemoveAsync(string key)
        {
            cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
