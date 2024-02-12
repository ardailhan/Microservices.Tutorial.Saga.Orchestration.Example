using MassTransit;
using SagaStateMachine.Service.StateInstances;

namespace SagaStateMachine.Service.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public OrderStateMachine()
        {
            InstanceState(instance => instance.CurrentState);
        }
    }
}
