using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;

namespace bulk_order_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BulkOrdersController : ControllerBase
    {
        private readonly ILogger<BulkOrdersController> _logger;
        private readonly IBulkOrderService _bulkOrderService;

        public BulkOrdersController(ILogger<BulkOrdersController> logger, IBulkOrderService bulkOrderService)
        {
            _logger = logger;
            _bulkOrderService = bulkOrderService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBulkOrders([FromBody] BulkCreateRequest req)
        {
            if (req == null || req.Items == null) return BadRequest();

            var res = (await _bulkOrderService.BulkCreateOrders(req.Items.ToArray()))
                .Select(res =>
                    res.Match(
                        ifLeft: err => new BulkCreateResponse(err),
                        ifRight: succ => new BulkCreateResponse(succ)))
                        .ToList();
            return Ok(res);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateBulkOrders([FromBody] BulkUpdateRequest req)
        {
            if (req == null) return BadRequest();

            var res = (await _bulkOrderService.BulkUpdateOrders(req.Items.ToArray()))
                .Select(res =>
                    res.Match(
                        ifLeft: err => new BulkCreateResponse(err),
                        ifRight: succ => new BulkCreateResponse(succ)))
                        .ToList();
            return Ok(res);
        }
    }
}
