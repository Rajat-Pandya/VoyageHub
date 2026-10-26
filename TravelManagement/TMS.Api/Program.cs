using Microsoft.EntityFrameworkCore;
using TMS.Api.Mapping;
using TMS.Business.Services;
using TMS.Data;
using TMS.Data.Entity;
using TMS.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1. Register services in the DI container ----------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Angular Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// DbContext — use SQL Server with connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository & Business Service registration
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ITravelRequestService, TravelRequestService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(MapperProfile));

var app = builder.Build();

// Auto-create database & table if not exists, and seed initial records if empty
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.TravelRequests.Any())
    {
        db.TravelRequests.AddRange(
            new TravelRequest
            {
                EmployeeName = "Rajat Sharma",
                Destination = "Mumbai",
                StartDate = DateTime.UtcNow.AddDays(15),
                EndDate = DateTime.UtcNow.AddDays(18),
                Purpose = "Client meeting — Q4 contract review",
                EstimatedBudget = 45000,
                Status = "Manager Approved",
                CreatedOn = DateTime.UtcNow.AddDays(-2)
            },
            new TravelRequest
            {
                EmployeeName = "Rajat Sharma",
                Destination = "Bangalore",
                StartDate = DateTime.UtcNow.AddDays(25),
                EndDate = DateTime.UtcNow.AddDays(27),
                Purpose = "Annual Engineering & Tech Summit",
                EstimatedBudget = 32000,
                Status = "Draft",
                CreatedOn = DateTime.UtcNow.AddDays(-1)
            },
            new TravelRequest
            {
                EmployeeName = "Neha Gupta",
                Destination = "Hyderabad",
                StartDate = DateTime.UtcNow.AddDays(20),
                EndDate = DateTime.UtcNow.AddDays(22),
                Purpose = "Key Enterprise Client Demo & Pitch",
                EstimatedBudget = 18500,
                Status = "Submitted",
                CreatedOn = DateTime.UtcNow.AddDays(-3)
            },
            new TravelRequest
            {
                EmployeeName = "Rohan Mehta",
                Destination = "Singapore",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(35),
                Purpose = "APAC Partner Summit keynote presentation",
                EstimatedBudget = 84000,
                Status = "Finance Approved",
                CreatedOn = DateTime.UtcNow.AddDays(-5)
            }
        );
        db.SaveChanges();
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Database EnsureCreated/Seed encountered an error. Check SQL Server connectivity.");
}

// ---------- 2. Configure HTTP request pipeline ----------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularClient");
app.UseAuthorization();
app.MapControllers();

app.Run();
