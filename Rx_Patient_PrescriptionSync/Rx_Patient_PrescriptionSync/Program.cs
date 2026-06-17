using Rx_Patient_PrescriptionSync;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Register our decoupled security validation logic
builder.Services.AddSingleton<TokenValidatorService>();

var app = builder.Build();

app.UseMiddleware(typeof(IdentityGatewayMiddleware));


// TEST TOOL ENDPOINT: Simulates Rexall's centralized OAuth Identity Server generating a token
app.MapGet("/api/get-token", (IConfiguration config) =>
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(config["IdentitySettings:SecretKey"]!);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, config["IdentitySettings:RequiredRole"]!),
            new Claim("location_id", "Store-Mississauga-0412"),
            new Claim("scope", "prescriptions.sync")
        }),
        Expires = DateTime.UtcNow.AddMinutes(60), // Short lifespan for security compliance
        Issuer = config["IdentitySettings:Issuer"],
        Audience = config["IdentitySettings:Audience"],
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return new { Token = tokenHandler.WriteToken(token) };
});

// SECURED RESOURCE ENDPOINT: High-risk patient prescription queue
app.MapGet("/api/v1/pharmacy/dispense-queue", () => new[]
{
    new { PrescriptionId = "RX-2026-991A", PatientInitials = "J.D.", DrugName = "Amoxicillin", Dosage = "500mg", Quantity = 21, Status = "Awaiting Packaging" },
    new { PrescriptionId = "RX-2026-440B", PatientInitials = "M.S.", DrugName = "Metformin", Dosage = "850mg", Quantity = 90, Status = "Verified by Pharmacist" }
});

app.Run();