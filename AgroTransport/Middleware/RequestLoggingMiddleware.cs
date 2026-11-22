using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AgroTransport.Middleware
{
    /// <summary>
    /// Custom middleware demonstrating ASP.NET Core request pipeline processing (Course: INT402 - Unit IV).
    /// Logs incoming request method, path, execution time, and authenticated user identity.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var user = context.User.Identity?.IsAuthenticated == true
                    ? context.User.Identity.Name
                    : "Anonymous";

                var statusCode = context.Response.StatusCode;
                _logger.LogInformation(
                    "[INT402 RequestLog] {Method} {Path} -> Status: {StatusCode} in {ElapsedMs}ms (User: {User})",
                    request.Method,
                    request.Path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    user);
            }
        }
    }
}

