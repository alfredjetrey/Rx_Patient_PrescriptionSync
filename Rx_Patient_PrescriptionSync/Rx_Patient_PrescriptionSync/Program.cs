using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Rx_Patient_PrescriptionSync;
using Rx_Patient_PrescriptionSync.Model;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Register our decoupled security validation logic
builder.Services.AddSingleton<TokenValidatorService>();

var app = builder.Build();

app.UseMiddleware(typeof(IdentityGatewayMiddleware));


app.MapPost("/api/get-token", (TokenRequestModel request, IConfiguration config) =>
{
    // 1. Validate Machine Credentials (Simulating a database lookup)
    var validSecret = config["IdentitySettings:SecretKey"]!;

    // Simple verification check: in production, you would check a database of registered bots
    if (request.ClientId != "RexallBot-01" || request.ClientSecret != validSecret)
    {
        return Results.Json(new { Error = "invalid_client", Message = "Authentication failed." }, statusCode: 401);
    }

    // 2. Build the Token Generation Infrastructure
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(validSecret);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        //  THE DYNAMIC CLAIMS ENGINE: Values are injected straight from the client request
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, config["IdentitySettings:RequiredRole"]!), // Enforces AutomatedDispenserBot
            new Claim("location_id", request.RequestedLocation),                  // Dynamically injected location
            new Claim("scope", request.RequestedScope)                            // Dynamically injected scope
        }),
        Expires = DateTime.UtcNow.AddMinutes(30),
        Issuer = config["IdentitySettings:Issuer"],
        Audience = config["IdentitySettings:Audience"],
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);

    // Return standard OAuth-compliant response format
    return Results.Ok(new
    {
        access_token = tokenHandler.WriteToken(token),
        token_type = "Bearer",
        expires_in = 1800
    });
});

// SECURED RESOURCE ENDPOINT: High-risk patient prescription queue
app.MapGet("/api/v1/pharmacy/dispense-queue", () => new[]
{
    new { PrescriptionId = "RX-2026-991A", PatientInitials = "J.D.", DrugName = "Amoxicillin", Dosage = "500mg", Quantity = 21, Status = "Awaiting Packaging" },
    new { PrescriptionId = "RX-2026-440B", PatientInitials = "M.S.", DrugName = "Metformin", Dosage = "850mg", Quantity = 90, Status = "Verified by Pharmacist" }
});

app.Run();