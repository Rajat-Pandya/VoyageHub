using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using VoyageHub.Api.Services;
using VoyageHub.Business.Services;
using VoyageHub.Data;
using VoyageHub.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configuration validation & fail-fast startup checks (never log values)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VoyageHub.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VoyageHub.Client";

if (!builder.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Startup failed: Database connection string 'ConnectionStrings:DefaultConnection' is missing or not configured.");
    }

    if (string.IsNullOrWhiteSpace(jwtKey))
    {
        throw new InvalidOperationException("Startup failed: JWT signing key ('Jwt:Key') is missing or not configured.");
    }

    if (jwtKey.Length < 32)
    {
        throw new InvalidOperationException($"Startup failed: JWT signing key ('Jwt:Key') must be at least 32 characters long (found {jwtKey.Length}).");
    }
}
else
{
    // Fallback for local development when not yet in user-secrets or appsettings
    jwtKey ??= "VoyageHubSuperSecretKeyForJwtAuthenticationTokenSigning2026!";
    connectionString ??= "Server=(localdb)\\MSSQLLocalDB;Database=AngularAuthDb;Trusted_Connection=True;TrustServerCertificate=True;";
}

// 1. Add DbContext with SQL Server Connection & Transient Error Resiliency
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    ));

// HttpContext accessor for extracting user identity in scoped services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Services & Repositories (All Scoped to share one AppDbContext per request)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IApprovalStepRepository, ApprovalStepRepository>();
builder.Services.AddScoped<ITravelRequestRepository, TravelRequestRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<ISupportService, SupportService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<ITravelRequestService, TravelRequestService>();

// 2. JWT Authentication Configuration

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.Name
    };
});

builder.Services.AddAuthorization();

// 3. Add Controllers & JSON casing options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 4. Configure CORS policy for Angular frontend
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

if (!builder.Environment.IsDevelopment())
{
    var envOrigins = Environment.GetEnvironmentVariables().Keys
        .Cast<string>()
        .Where(k => k.StartsWith("Cors__AllowedOrigins", StringComparison.OrdinalIgnoreCase) ||
                    k.StartsWith("Cors:AllowedOrigins", StringComparison.OrdinalIgnoreCase))
        .Select(k => Environment.GetEnvironmentVariable(k))
        .Where(v => !string.IsNullOrWhiteSpace(v))
        .Select(v => v!)
        .ToArray();

    if (envOrigins.Length > 0)
    {
        corsOrigins = envOrigins;
    }
    else
    {
        corsOrigins = corsOrigins.Where(o => !o.Contains("localhost", StringComparison.OrdinalIgnoreCase) && !o.Contains("127.0.0.1")).ToArray();
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins);
        }
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 5. OpenAPI & Swagger UI with JWT Bearer Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VoyageHub API",
        Version = "v1",
        Description = "VoyageHub Corporate Travel & Expense Management API with JWT Bearer Authentication"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token format: Bearer {your token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var app = builder.Build();

var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled", builder.Environment.IsDevelopment());

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoyageHub API v1");
        c.RoutePrefix = "swagger";
    });
}

// Redirect root to Swagger UI when enabled, otherwise return 200 text response
app.MapGet("/", () => swaggerEnabled
    ? Results.Redirect("/swagger")
    : Results.Text("VoyageHub API is running.", "text/plain"));

app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
