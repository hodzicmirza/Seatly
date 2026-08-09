using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Seatly.Application.Interfaces;
using Seatly.Application.Services;
using Seatly.Application.Services.Discounts;
using Seatly.Domain.Interfaces;
using Seatly.Infrastructure.Data;
using Seatly.Infrastructure.Repositories;
using Seatly.Infrastructure.Services;
using Seatly.API.BackgroundServices;

namespace Seatly.API;

public class Program
{
    public static void Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMemoryCache();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // ============ REPOSITORIES ============
        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // ============ APPLICATION SERVICES ============
        builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<IUserService, UserService>();

        // ============ DISCOUNT STRATEGIES ============
        builder.Services.AddSingleton<IDiscountStrategy, EarlyBirdDiscount>();
        builder.Services.AddSingleton<IDiscountStrategy, VIPDiscount>();
        builder.Services.AddSingleton<IDiscountStrategy, BulkDiscount>();

        // ============ INFRASTRUCTURE SERVICES ============
        var emailApiKey = builder.Configuration["Email:ApiKey"] ?? "";
        var senderEmail = builder.Configuration["Email:SenderEmail"] ?? "";
        builder.Services.AddSingleton<IEmailService>(new EmailService(emailApiKey, senderEmail));
        builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
        builder.Services.AddHostedService<ExpiredBookingCleanupService>();

        // ============ AUTHENTICATION ============
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Supabase:Authority"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Supabase:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Supabase:ValidAudience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    TryAllIssuerSigningKeys = true,
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context => Task.CompletedTask,
                    OnTokenValidated = context => Task.CompletedTask,
                    OnChallenge = context => Task.CompletedTask,
                };
            });

        builder.Services.AddAuthorization();

        // ============ CONTROLLERS ============
        builder
            .Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = System
                    .Text
                    .Json
                    .JsonNamingPolicy
                    .CamelCase;
            });

        // ============ SWAGGER ============
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Seatly API",
                    Version = "v1",
                    Description = "Event Booking System with QR Code Tickets",
                }
            );

            c.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                }
            );

            c.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );
        });

        // ============ CORS ============
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowFrontend",
                policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:5173",
                            "http://localhost:5174"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            );
        });

        var app = builder.Build();

        // ============ MIDDLEWARE PIPELINE ============
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Seatly API v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseMiddleware<Seatly.API.Middleware.ExceptionMiddleware>();
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Auto-migrate database
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        app.Logger.LogInformation("Seatly API v1.0 running successfully.");

        app.Run();
    }
}
