using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();
        Task<IEnumerable<OrderSummary>> GetFailedOrdersAsync();
        Task<IEnumerable<OrderSummary>> GetCompletedOrdersAsync();

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, string newState);

        Task<Guid> CreateOrderAsync(OrderCreateDto dto);

    }
}
