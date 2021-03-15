using System;
using System.Linq;
using System.Collections.Generic;
using Common.DataAccess;
using Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;


namespace Common.Services
{
    public class OrderService : IOrderService
    {
        private readonly IMassiveOrmDataAccess _dataAccess;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IMassiveOrmDataAccess dataAccess, ILogger<OrderService> logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public bool CancelOrder(Guid id)
        {
            return _dataAccess.FromTable("orders").Update(new { status = "Cancelled" }, new { id }) != 0;
        }

        public async Task<OrderCreated> CreateOrder(CreateOrder newOrder)
        {
            var inserted = await Task.Run(() => _dataAccess
                .FromTable("orders")
                .Insert(new
                {
                    id = Guid.NewGuid(),
                    status = "Order placed"
                }));

            return inserted is object ? newOrder.ToCreatedOrder((Guid)inserted.id, inserted.status) : null;
        }

        public OrderCreated GetOrderById(Guid id) =>
            _dataAccess
                .FromTable("orders")
                .All(where: $"id = '{id}'")
                .Select<dynamic, OrderCreated>(x => OrderCreated.FromDynamic(x, _logger))
                .FirstOrDefault();

        public List<OrderCreated> GetOrders() =>
            _dataAccess
                .FromTable("orders")
                .All()
                .Select<dynamic, OrderCreated>(x => OrderCreated.FromDynamic(x, _logger))
                .ToList();

    }

    public interface IOrderService
    {
        List<OrderCreated> GetOrders();
        OrderCreated GetOrderById(Guid id);
        Task<OrderCreated> CreateOrder(CreateOrder newOrder);
        bool CancelOrder(Guid id);
    }
}