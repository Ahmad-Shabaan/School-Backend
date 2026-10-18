using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Events
{
    public class OrderCreatedDomainEvent
    {
        public int OrderId { get; }
        public OrderCreatedDomainEvent(int orderId)
        {
            OrderId = orderId;
        }
    }
}
