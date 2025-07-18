﻿using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;
        private readonly string failedStatus = "Failed";    // use private string to avoid magic value
        private readonly string completedStatus = "Completed";

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<IEnumerable<OrderSummary>> GetFailedOrdersAsync()
        {

            var failedOrders = await GetOrdersByStatusQuery(this.failedStatus)
            .ToListAsync();

            return failedOrders;
        }

        public async Task<IEnumerable<OrderSummary>> GetCompletedOrdersAsync()
        {

            var failedOrders = await GetOrdersByStatusQuery(this.completedStatus)
            .ToListAsync();

            return failedOrders;
        }

        private IQueryable<OrderSummary> GetOrdersByStatusQuery(string status)
        {
            return _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Where(x => x.Status.Name == status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate);
        }


        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();
            
            return order;
        }

        /// <summary>
        /// Assigns new OrderStatus (as a navigation property) to an Order entity
        /// </summary>
        /// <remarks>
        /// The new OrderStatus is looked up by a name match
        /// </remarks>
        public async Task UpdateOrderStatusAsync(Guid orderId, string newState)
        {
            // try to find state in the db by its name 
            var status = await _orderContext.OrderStatus
                .SingleOrDefaultAsync(x => x.Name.ToLower() == newState.ToLower());

            if (status == null)
                throw new InvalidOperationException($"Status '{newState}' not found in the db.");

            // find the order int the db
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .SingleOrDefaultAsync();

            if (order == null)
                throw new InvalidOperationException($"Order with ID '{orderId}' not found.");

            // set the new status to the order 
            order.Status = status;
            await _orderContext.SaveChangesAsync();
        }

        /// <summary>
        /// Async add new Order entity into the db. 
        /// Do not allow non-existing Status navigation property (throw exception). 
        /// </summary>
        /// <remarks>
        /// Non-existing Customer or Reseller should also not be allowed, but their entities are not part of this test-project at the moment
        /// </remarks>
        /// <returns>Id of the new order. </returns>
        public async Task<Guid> CreateOrderAsync(OrderCreateDto dto)
        {
            var status = await _orderContext.OrderStatus
                .FirstOrDefaultAsync(x => x.Id == dto.StatusId.ToByteArray());

            if (status == null)
                throw new InvalidOperationException($"Status '{dto.StatusId}' not found in the db.");

            // TODO: Validate Reseller and Customer existence in DB once those entities are implemented (they were not part of the tech-test).
            // For now, only Status is checked to avoid breaking the current test-project scope.

            var newGuid = Guid.NewGuid();

            var order = new Order.Data.Entities.Order
            {
                Id = newGuid.ToByteArray(),
                ResellerId = dto.ResellerId.ToByteArray(),
                CustomerId = dto.CustomerId.ToByteArray(),
                CreatedDate = dto.CreatedDate,
                Status = status
            };

            await _orderContext.Order.AddAsync(order);
            await _orderContext.SaveChangesAsync();

            return newGuid;
        }
    }
}
