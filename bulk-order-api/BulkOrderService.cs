using System;
using System.Linq;
using System.Threading.Tasks;
using Common.DataAccess;
using Common.Services;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;

public interface IBulkOrderService
{
    Task<Either<Exception, bool>[]> BulkCreateOrders(CreateOrder[] newOrder);
    Task<Either<Exception, bool>[]> BulkUpdateOrders(UpdateOrderStatus[] orderUpdates);
}

public class BulkOrderService : IBulkOrderService
{
    private readonly IMassiveOrmDataAccess _dataAccess;
    private readonly IOrderService _orderService;
    private readonly ILogger<BulkOrderService> _logger;
    private readonly ICreateOrderPublisher _publisher;
    public BulkOrderService(IMassiveOrmDataAccess dataAccess, IOrderService orderService, ILogger<BulkOrderService> logger, ICreateOrderPublisher publisher)
    {
        _dataAccess = dataAccess;
        _logger = logger;
        _publisher = publisher;
        _orderService = orderService;
    }

    public async Task<Either<Exception, bool>[]> BulkCreateOrders(CreateOrder[] newOrders)
    {
        if (newOrders == null) throw new ArgumentNullException("newOrders");

        var inserts = newOrders.Select(async item =>
        {
            try
            {
                var canAccept = await _orderService.CanAcceptOrder(item);

                if (canAccept) _publisher.Publish(item);

                return new Either<Exception, bool>(canAccept);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "failed to create order");

                return new Either<Exception, bool>(ex);
            }
        })
        .ToList();

        return await Task.WhenAll(inserts);
    }

    public Task<Either<Exception, bool>[]> BulkUpdateOrders(UpdateOrderStatus[] orderUpdates)
    {
        if(orderUpdates == null) throw new ArgumentNullException("orderUpdates");
        var updateTasks = orderUpdates.Select(async item =>
       {
           try
           {
               var id = item.OrderId;
               var status = item.Status;
               var updateSqlQuery = $@"UPDATE [dbo].[orders] SET status = '{status}' WHERE id = '{id}'";
               using (var conn = _dataAccess.FromTable("orders").OpenConnection())
               {
                   var cmd = conn.CreateCommand();
                   cmd.CommandText = updateSqlQuery;
                   var affectedRows = await cmd.ExecuteNonQueryAsync();

                   return new Either<Exception, bool>(affectedRows == 1);
               }
           }
           catch (System.Exception ex)
           {
               _logger.LogError(ex, "failed to update order");
               return new Either<Exception, bool>(ex);
           }
       }).ToArray();

        return Task.WhenAll(updateTasks);
    }
}
