using System.Text.Json.Serialization;
using Application.Middlewares;
using Application.Service;
using Application.Workers;
using Core.Cloudinary;
using Core.Crypto;
using Core.Elasticsearch;
using Core.Firebase;
using Core.Jwt;
using Core.LLM;
using Core.Mail;
using Core.Payos;
using Core.Queue;
using Core.Realtime;
using Database;
using Infrastructure.Cache;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MuseTrip360.src.Application.Service;
using StackExchange.Redis;

var CORS = "AllowAllOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Other JSON options you might have...
    }); ;
builder.Services.AddHttpClient();
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnection") ?? "");
});

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection") ?? throw new ArgumentNullException("RedisConnection")));

// Elasticsearch Configuration
builder.Services.Configure<ElasticsearchConfiguration>(
    builder.Configuration.GetSection("Elasticsearch"));
builder.Services.AddSingleton<IElasticsearchService, ElasticsearchService>();

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MuseTrip360 API", Version = "v1" });

    // Add XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

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
builder.Services.AddSingleton<IRealtimeService, RealtimeService>();
builder.Services.AddSingleton<RabbitMQConnection, RabbitMQConnection>();
builder.Services.AddSingleton<IQueuePublisher, RabbitMqPublisher>();
builder.Services.AddSingleton<IQueueSubscriber, RabbitMqSubscriber>();
builder.Services.AddSingleton<ILLM, GeminiSvc>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMuseumService, MuseumService>();
builder.Services.AddScoped<IArtifactService, ArtifactService>();
builder.Services.AddScoped<IRolebaseService, RolebaseService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAdminEventService, AdminEventService>();
builder.Services.AddScoped<IOrganizerEventService, OrganizerEventService>();
builder.Services.AddScoped<IEventParticipantService, EventParticipantService>();
builder.Services.AddScoped<ISearchItemService, SearchItemService>();

builder.Services.AddScoped<ITourOnlineService, TourOnlineService>();
builder.Services.AddScoped<IAdminTourOnlineService, TourOnlineAdminService>();
builder.Services.AddScoped<ITourContentService, TourContentService>();
builder.Services.AddScoped<IAdminTourContentService, AdminTourContentService>();
builder.Services.AddScoped<ITourGuideService, TourGuideService>();
builder.Services.AddScoped<IAdminTourGuideService, AdminTourGuideService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRepresentationMaterialService, RepresentationMaterialService>();
// Singleton for RoomStateManager
builder.Services.AddSingleton<IRoomStateManager, RoomStateManager>();
// Workers
builder.Services.AddHostedService<NotificationWorker>();
builder.Services.AddHostedService<OrderWorker>();
var app = builder.Build();

app.UseInitializeDatabase();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// app.UseStaticFiles();
// app.UseRouting();
app.UseCors(CORS);
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseWebSockets();
app.MapHub<ChatHub>("/chat").RequireCors(CORS);
app.MapHub<SignalingHub>("/signaling");
app.MapControllers();

app.Run();
