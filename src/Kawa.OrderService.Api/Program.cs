using Kawa.OrderService.Api.Services;
using Kawa.OrderService.Api.Models;
using Kawa.OrderService.Api.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddServiceDefaults();
builder.AddRabbitMQClient("messaging");

// Ajouter les contr�leurs � l'application
builder.Services.AddControllers();


// Ajouter les services de messagerie
builder.Services.AddScoped<IMessageBrokerService, MessageBrokerService>();
builder.Services.AddScoped<IMessageHandler<OrderMessage>, OrderMessageHandler>();
builder.Services.AddHostedService<MessageConsumerService>();

builder.Services.AddDbContext<CommandesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("order-service-db")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CommandesDbContext>("Database")
    .AddRabbitMQ(builder.Configuration.GetConnectionString("messaging"), name: "rabbitmq");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();

public partial class Program { }