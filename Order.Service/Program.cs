using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Service.Data;
using ECommerce.Contracts;
using Order.Service.Consumers;

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
    //tells MassTransit to look for your new consumer class and set up a queue for it.
    x.AddConsumer<PaymentInitiatedConsumer>();

    // Configure the Entity Framework Outbox
    x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox(); 
    });

    // CRITICAL ADDITION: This middleware ensures the Outbox is used for all endpoints
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

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin() // In production, specify your domain
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 5. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();
app.Run();