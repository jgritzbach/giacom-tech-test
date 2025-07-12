using Order.Data;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Order.Data.Entities;
using System.Linq;

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

        /// <summary>
        /// Groups all completed orders by year and month. 
        /// Counts Profit (price - sum) for each month. 
        /// </summary>
        /// <returns>A list of monthly profits as DTOs. </returns>
        public async Task<List<OrderMonthlyProfitDto>> GetProfitByMonthAsync()
        {

            var completedOrders = await _orderRepository.GetCompletedOrdersAsync();

            var groupedProfits = completedOrders
                .GroupBy(o => new { o.CreatedDate.Year, o.CreatedDate.Month })
                .Select(g => new OrderMonthlyProfitDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Profit = g.Sum(o => o.TotalPrice - o.TotalCost)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            return groupedProfits;

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
