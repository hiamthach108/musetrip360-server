using System.Text.Json.Serialization;
using Application.Service;
using Core.Cloudinary;
using Core.Crypto;
using Core.Firebase;
using Core.Jwt;
using Core.Mail;
using Core.Payos;
using Core.Realtime;
using Database;
using Infrastructure.Cache;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var CORS = "AllowAllOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        // Other JSON options you might have...
    }); ;
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddBearerToken();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddDbContext<MuseTrip360DbContext>(options =>
{
    // log the connection string
    Console.WriteLine($"Connection string: {builder.Configuration.GetConnectionString("DatabaseConnection")}");
    options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnection") ?? "");
});

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection") ?? throw new ArgumentNullException("RedisConnection")));


builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddCors(options =>
{
    options.AddPolicy(CORS,
        builder =>
        {
            builder.SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration.GetValue<string>("GoogleClientId") ?? throw new ArgumentNullException("GoogleClientId");
    options.ClientSecret = builder.Configuration.GetValue<string>("GoogleClientSecret") ?? throw new ArgumentNullException("GoogleClientSecret");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rescue & 360 Furniture API", Version = "v1" });

    // Add a bearer token to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    // Require the bearer token for all API operations
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
      {
          new OpenApiSecurityScheme
          {
              Reference = new OpenApiReference
              {
                  Type = ReferenceType.SecurityScheme,
                  Id = "Bearer"
              }
          },
          new string[] {}
      }
    });
});

// Core
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<ICryptoService, CryptoService>();
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.AddSingleton<IFirebaseAdminService, FirebaseAdminService>();
builder.Services.AddSingleton<IMailService, MailService>();
builder.Services.AddSingleton<IPayOSService, PayOSService>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMuseumService, MuseumService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// app.UseStaticFiles();
app.UseRouting();
app.UseCors(CORS);
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapHub<ChatHub>("/chat").RequireCors(CORS);
app.MapControllers();

app.Run();
