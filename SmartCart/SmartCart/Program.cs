using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using SmartCart.Core.Implementation.Service.Domain;
using SmartCart.Core.Interfaces;
using SmartCart.Core.Interfaces.IServices.Application;
using SmartCart.Core.Interfaces.IServices.Domain;
using SmartCart.Hubs;
using SmartCart.Infrastructure.Data;
using SmartCart.Infrastructure.Data.Seed;
using SmartCart.Infrastructure.Services;
using SmartCart.Infrastructure.UnitOfWork;
using SmartCart.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Json.NET ───────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

// ── Swagger ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartCart API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT Bearer token."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<SmartCartDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Redis ──────────────────────────────────────────────────────────────────
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(redisConnection));
    builder.Services.AddScoped<ICacheService, CacheService>();
}
else
{
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ICacheService, MemoryCacheService>();
}

// ── JWT Authentication ─────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Allow JWT via SignalR query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── CORS (for Angular Static Web App) ─────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("SmartCartPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── SignalR ────────────────────────────────────────────────────────────────
builder.Services.AddSignalR()
    .AddNewtonsoftJsonProtocol();

// ── Repository / UoW ──────────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── Core Domain Services ───────────────────────────────────────────────────
builder.Services.AddScoped<ICartDomainService, CartDomainService>();
builder.Services.AddScoped<ICartSessionDomainService, CartSessionDomainService>();

// ── Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICartScanService, CartScanService>();
builder.Services.AddSingleton<IOtpService, OtpService>();
builder.Services.AddScoped<ICartNotificationService, CartNotificationService>();

// ── EventHub Background Service ────────────────────────────────────────────
builder.Services.AddHostedService<EventHubConsumerService>();

var app = builder.Build();

// ── Database Seed ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartCartDbContext>();
    await DataSeeder.SeedAsync(db);
}

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("SmartCartPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CartHub>("/hubs/cart");

app.Run();
