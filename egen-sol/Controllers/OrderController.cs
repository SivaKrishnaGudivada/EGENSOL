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
        private readonly ICreateOrderPublisher _publisher;

        public OrdersController(ILogger<OrdersController> logger, IOrderService orderService, ICreateOrderPublisher publisher)
        {
            _logger = logger;
            _orderService = orderService;
            _publisher = publisher;
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

            var canAccept = await _orderService.CanAcceptOrder(newOrder);

            if (canAccept) 
            {
                _logger.LogInformation("Can accept this order by cust: " + newOrder.CustomerId);
                _publisher.Publish(newOrder);
                return Accepted();
            }

            return BadRequest();
        }

        [HttpPut("cancel/{id}")] // default route to post creates a resource
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var updated = await _orderService.CancelOrder(id);
            if (updated) 
            {
                _logger.LogInformation("Order cancelled: "+id);
                return NoContent();
            }

            return BadRequest();
        }
    }
}