using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Service.Data;
using ECommerce.Contracts;
using Order.Service.Consumers;
using Order.Service.Hubs;
using Order.Service.Workers;

var builder = WebApplication.CreateBuilder(args);

// 1. Standard API setup
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Configuration strings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");

// 3. Database context setup
builder.Services.AddDbContext<OrderDbContext>(x => 
    x.UseSqlServer(connectionString));

// 4. MassTransit Configuration
builder.Services.AddMassTransit(x =>
{
    // 1. Existing Consumer
    x.AddConsumer<PaymentInitiatedConsumer>();

    // 2. NEW CONSUMER ADDITION: Listens for PaymentCompleted from Payment.Service
    x.AddConsumer<PaymentCompletedConsumer>();

    // Configure the Entity Framework Outbox
    x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox(); 
    });

    // Ensures the Outbox is used for all endpoints (including the new one)
    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<OrderDbContext>(context);
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqConfig["Host"] ?? "localhost", "/", h => {
            h.Username(rabbitMqConfig["Username"] ?? "guest");
            h.Password(rabbitMqConfig["Password"] ?? "guest");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

// CRITICAL ADDITION: This starts the background "delivery truck" service 
// to push messages from SQL to RabbitMQ.
builder.Services.AddOptions<MassTransitHostOptions>()
    .Configure(options => options.WaitUntilStarted = true);

builder.Services.AddSignalR();

// Fetch the URL from appsettings
var frontendUrl = builder.Configuration["FrontendSettings:Url"] ?? "http://127.0.0.1:5500";

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins(frontendUrl) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHostedService<PaymentTimeoutWorker>();

var app = builder.Build();

// --- Database Migration Automation ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OrderDbContext>();
        
        // This ensures the database exists and all tables are created.
        // It uses the volume data if present, or creates it if missing.
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            Console.WriteLine("Database Migration: Successfully applied pending migrations.");
        }
        else
        {
            Console.WriteLine("Database Migration: Already up to date.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database Migration Error: {ex.Message}");
    }
}
// -------------------------------------

// 5. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting(); // Important: Define routing before CORS
app.UseCors();
app.MapControllers();
app.MapHub<OrderHub>("/orderHub");
app.Run();