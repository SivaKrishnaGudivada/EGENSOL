using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Common.Services;

namespace egen_sol.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IOrderService _orderService;

        public OrdersController(ILogger<OrdersController> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [HttpGet] // default route to get returns all the order objects
        public JsonResult GetAllOrders() =>
            new JsonResult(_orderService.GetOrders());

        [HttpGet("{id}")] // default route to get returns all the order objects
        public JsonResult GetOrderById(Guid id) =>
            new JsonResult(_orderService.GetOrderById(id));

        [HttpPost("")] // default route to post creates a resource
        public async Task<IActionResult> Create([FromBody] CreateOrder newOrder)
        {
            if (newOrder == null) return BadRequest();

            var newOrderCreated = await _orderService.CreateOrder(newOrder);

            if (newOrderCreated != null) return Ok(newOrderCreated);

            return BadRequest();
        }

        [HttpPut("cancel/{id}")] // default route to post creates a resource
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var updated = await _orderService.CancelOrder(id);
            if (updated) return NoContent();

            return BadRequest();
        }
    }
}