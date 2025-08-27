using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxIDs.SampleHelperLibrary.Middleware
{
    public class RawRequestLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RawRequestLoggingMiddleware> logger;

        public RawRequestLoggingMiddleware(RequestDelegate next, ILogger<RawRequestLoggingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

            var headers = string.Join("\n", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"));
            string bodyText = "<no body>";
            if (context.Request.ContentLength != null && context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                bodyText = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            logger.LogInformation("RAW HTTP REQUEST\n{Method} {Path}{Query}\n{Headers}\n\n{Body}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                headers,
                Truncate(bodyText, 4000));

            await next(context);
        }

        private static string Truncate(string value, int max) =>
            value != null && value.Length > max ? value.Substring(0, max) + "...(truncated)" : value ?? "";
    }
}
