using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Data.Entities;
using Order.Model;
using Order.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        [HttpGet]
        [Route("failed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFailedOrdersAsync()
        {
            var failedOrders = await _orderService.GetFailedOrdersAsync();
            return Ok(failedOrders);
        }

        [HttpGet]
        [Route("profit-by-month")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<OrderMonthlyProfitDto>>> GetProfitByMonthAsync()
        {
            var result = await _orderService.GetProfitByMonthAsync();

            if (result == null || !result.Any())
            {
                return Ok(new List<OrderMonthlyProfitDto>());
            }

            return Ok(result);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPatch("{orderId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatusAsync(Guid orderId, OrderChangeStateDto dto)
        {
            await _orderService.UpdateOrderStatusAsync(orderId, dto.NewStatusName);
            return Ok();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrderAsync(OrderCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdOrderId = await _orderService.CreateOrderAsync(dto);

            // Return 201 Created with URI to the new order
            return Ok(createdOrderId);
        }
    }
}
