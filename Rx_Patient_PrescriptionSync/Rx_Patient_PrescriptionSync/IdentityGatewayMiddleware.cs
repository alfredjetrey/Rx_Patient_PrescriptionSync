using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Rx_Patient_PrescriptionSync
{
 
    public class IdentityGatewayMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenValidatorService _validator;
        private readonly IConfiguration _configuration;

        public IdentityGatewayMiddleware(RequestDelegate next, TokenValidatorService validator, IConfiguration configuration)
        {
            _next = next;
            _validator = validator;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Bypass security pipeline ONLY for our local token generator endpoint
            if (context.Request.Path.StartsWithSegments("/api/get-token"))
            {
                await _next(context);
                return;
            }

            // 1. Verify Authorization Header Presence
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Rexall Gateway Error: Missing Authorization Header.");
                return;
            }

            // 2. Verify Bearer Token Formatting
            var tokenStr = authHeader.ToString();
            if (!tokenStr.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Rexall Gateway Error: Invalid Token Scheme. Expected Bearer.");
                return;
            }

            var token = tokenStr.Substring(7);

            // 3. Perform Cryptographic Verification
            if (!_validator.ValidateIncomingToken(token, out string? userRole))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Rexall Gateway Security Alert: Compromised, manipulated, or expired token.");
                return;
            }

            // 4. Enforce Contextual Policy-Based Access Control
            var requiredRole = _configuration["IdentitySettings:RequiredRole"] ?? "DefaultBot";
            if (userRole != requiredRole)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Rexall Gateway Security Alert: Access Denied. Machine identity unauthorized.");
                return;
            }

            // Everything checks out safely, execute downstream endpoint logic
            await _next(context);
        }

    }
}
