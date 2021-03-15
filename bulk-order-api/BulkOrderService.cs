using System;
using System.Linq;
using System.Threading.Tasks;
using Common.DataAccess;
using Common.Services;
using Microsoft.Extensions.Logging;
using Models;

public interface IBulkOrderService
{
    Task<Either<Exception, OrderCreated>[]> BulkCreateOrders(CreateOrder[] newOrder);
    Task<Either<Exception, bool>[]> BulkUpdateOrders(UpdateOrderStatus[] orderUpdates);
}

public class BulkOrderService : IBulkOrderService
{
    private readonly IMassiveOrmDataAccess _dataAccess;
    private readonly IOrderService _orderService;
    private readonly ILogger<BulkOrderService> _logger;
    private readonly IKafkaPublisher _publisher;
    public BulkOrderService(IMassiveOrmDataAccess dataAccess, IOrderService orderService, ILogger<BulkOrderService> logger, IKafkaPublisher publisher)
    {
        _dataAccess = dataAccess;
        _logger = logger;
        _publisher = publisher;
        _orderService = orderService;
    }

    public async Task<Either<Exception, OrderCreated>[]> BulkCreateOrders(CreateOrder[] newOrders)
    {
        var inserts = newOrders.Select(async item =>
        {
            try
            {
                var inserted = new Either<Exception, OrderCreated>(await _orderService.CreateOrder(item));
                _publisher.Publish(Constants.KafkaBulkOrderCreateTopicString, inserted);
                return inserted;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "failed to create order");

                return new Either<Exception, OrderCreated>(ex);
            }
        })
        .ToList();

        return await Task.WhenAll(inserts);
    }

    public Task<Either<Exception, bool>[]> BulkUpdateOrders(UpdateOrderStatus[] orderUpdates)
    {
        var updateTasks = orderUpdates.Select(async item =>
       {
           try
           {
               var updatedRows = await Task.Run(() =>
                  _dataAccess
                    .FromTable("ordershippingstatus")
                    .Update(
                        new 
                        { 
                            status = item.Status 
                        }, 
                        where: $"orderid = '{item.OrderId}'"));

               return new Either<Exception, bool>(updatedRows == 1);
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
