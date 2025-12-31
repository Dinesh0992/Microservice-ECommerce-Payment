using MassTransit;
using Payment.Service.Consumers;

var builder = WebApplication.CreateBuilder(args);

// CHANGE 1: Added standard Web API services for better hosting/logging
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddMassTransit(x =>
{
    // Ensure this consumer is registered!
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqConfig["Host"] ?? "localhost", "/", h => {
            h.Username(rabbitMqConfig["Username"] ?? "guest");
            h.Password(rabbitMqConfig["Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var frontendUrl = builder.Configuration["FrontendSettings:Url"] ?? "http://127.0.0.1:5500";

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins(frontendUrl) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();

// CHANGE 2: Enable Swagger for the Payment Service
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();
app.Run();