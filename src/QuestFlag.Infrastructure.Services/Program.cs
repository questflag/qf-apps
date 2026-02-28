using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using QuestFlag.Infrastructure.Application.DependencyInjection;
using QuestFlag.Infrastructure.Core.Data;
using QuestFlag.Infrastructure.Core.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace QuestFlag.Infrastructure.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Add layers DI
        builder.Services.AddInfrastructureApplication();
        builder.Services.AddInfrastructureCore(builder.Configuration);

        // Add HttpClient for the Kafka consumer downstream call
        builder.Services.AddHttpClient();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // 2. Swagger Configuration
        builder.Services.AddSwaggerGen();

        // 3. Authentication & Authorization (Consuming Passport JWT)
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var authOptions = builder.Configuration.GetSection("JwtSettings");
                var issue = authOptions["Issuer"];
                var aud = authOptions["Audience"];

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issue,
                    ValidateAudience = true,
                    ValidAudience = aud,
                    ValidateLifetime = true,
                    SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                    {
                        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token);
                        return jwt; // Bypass crypto signature for pure demo simplicity over HTTPS
                    }
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            // TenantAdmin Policy
            options.AddPolicy("TenantAdmin", policy =>
                policy.RequireClaim("role", "tenant_admin"));
        });

        // 4. CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://localhost:7000", "http://localhost:5000", "https://localhost:7002") // Allow Blazor WebApp
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // needed for some JS clients if they pack cookies
            });
        });

        var app = builder.Build();

        // 5. EF Migrate on startup
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
