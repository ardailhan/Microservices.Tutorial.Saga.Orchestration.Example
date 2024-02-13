using MassTransit;
using Order.API.Context;
using Shared.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderFailedEventConsumer(OrderDBContext orderDBContext) : IConsumer<OrderFailedEvent>
    {
        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            Models.Order order = await orderDBContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null) 
            {
                order.OrderStatus = Enums.OrderStatus.Fail;
                await orderDBContext.SaveChangesAsync();
            }
        }
    }
}
