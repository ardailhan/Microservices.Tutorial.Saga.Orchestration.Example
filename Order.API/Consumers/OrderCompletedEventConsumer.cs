using MassTransit;
using Order.API.Context;
using Shared.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderCompletedEventConsumer(OrderDBContext orderDBContext) : IConsumer<OrderCompletedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            Models.Order order = await orderDBContext.Orders.FindAsync(context.Message.OrderId);
            if(order != null)
            {
                order.OrderStatus =Enums.OrderStatus.Completed;
                await orderDBContext.SaveChangesAsync();
            }
        }
    }
}
