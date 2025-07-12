using Order.Data;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Order.Data.Entities;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return orders;
        }

        public async Task<IEnumerable<OrderSummary>> GetFailedOrdersAsync()
        {
            var failedOrders = await _orderRepository.GetFailedOrdersAsync();
            return failedOrders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string newState)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, newState);
        }

        public async Task<Guid> CreateOrderAsync(OrderCreateDto dto)
        {
            return await _orderRepository.CreateOrderAsync(dto);
        }
    }
}
