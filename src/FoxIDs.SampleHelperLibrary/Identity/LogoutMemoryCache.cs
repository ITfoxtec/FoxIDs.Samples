using System.Collections.Concurrent;
using System.Linq;

namespace FoxIDs.SampleHelperLibrary.Identity
{
    public class LogoutMemoryCache
    {
        public ConcurrentBag<string> List { get; private set; } = new ConcurrentBag<string>();

        public void Remove(string item)
        {
            List = new ConcurrentBag<string>(List.Except(new[] { item }));
        }

        public void Clear()
        {
            List = new ConcurrentBag<string>();
        }
    }
}
