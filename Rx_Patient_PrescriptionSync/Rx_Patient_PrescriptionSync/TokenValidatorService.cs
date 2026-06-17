
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Rx_Patient_PrescriptionSync
{
    public class TokenValidatorService
    {
        private readonly IConfiguration _configuration;

        public TokenValidatorService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool ValidateIncomingToken(string token, out string? userRole)
        {
            userRole = null;
            var tokenHandler = new JwtSecurityTokenHandler();

            var secretKey = _configuration["IdentitySettings:SecretKey"];
            var issuer = _configuration["IdentitySettings:Issuer"];
            var audience = _configuration["IdentitySettings:Audience"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("Gateway Configuration Error: Identity keys are missing from settings.");
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Enforce exact expiration for healthcare data
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                userRole = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
