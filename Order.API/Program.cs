using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Context;
using Order.API.ViewModels;
using Shared.OrderEvents;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddDbContext<OrderDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderVM model, OrderDBContext context,ISendEndpointProvider sendEndpointProvider) =>
{
    Order.API.Models.Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        OrderStatus = Order.API.Enums.OrderStatus.Suspend,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new Order.API.Models.OrderItem
        {
            Price = oi.Price,
            Count = oi.Count,
            ProductId = oi.ProductId
        }).ToList()
    };
    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    OrderStartedEvent orderStartedEvent = new()
    {
        BuyerId = model.BuyerId,
        OrderId = order.Id,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage
        {
            Price = oi.Price,
            Count = oi.Count,
            ProductId = oi.ProductId
        }).ToList()
    };

    var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue : {RabbitMQSettings.StateMachineQueue}"));
    await sendEndpoint.Send<OrderStartedEvent>(orderStartedEvent);
});

app.Run();
