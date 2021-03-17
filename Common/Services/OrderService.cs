using System;
using System.Linq;
using System.Collections.Generic;
using Common.DataAccess;
using Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


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

        public async Task<bool> CancelOrder(Guid id)
        {
            var updateSqlQuery = $@"UPDATE [dbo].[orders] SET status = 'Cancelled' WHERE id = '{id}'";
            using (var conn = _dataAccess.FromTable("orders").OpenConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = updateSqlQuery;
                var affectedRows = await cmd.ExecuteNonQueryAsync();
                if (affectedRows == 1) return true;
            }

            return false;
        }

        public async Task<OrderCreated> CreateOrder(CreateOrder newOrder)
        {

            var newGuid = Guid.NewGuid();

            await ProcessOrder(newGuid, newOrder);

            var inserted = _dataAccess
                .FromTable("orders")
                .All(where: $"id='{newGuid}'")
                .Select(x => new { id = (Guid)x.id, status = (string)x.status })
                .FirstOrDefault();

            return inserted is object ? newOrder.ToCreatedOrder((Guid)inserted.id, inserted.status) : null;
        }

        private async Task ProcessOrder(Guid newOrderGuid, CreateOrder newOrder)
        {
            if (!ProcessPayment(newOrder)) throw new Exception("Failed to process payment for order!");
            var utcNow = DateTime.UtcNow;
            using (var connection = _dataAccess.FromTable("").OpenConnection())
            {
                var sqlTran = connection.BeginTransaction();
                var command = connection.CreateCommand();
                command.Transaction = sqlTran;
                try
                {
                    var valuesString = string.Join(",", newOrder.Items.Select(item => $@"(
		   '{Guid.NewGuid()}',
		   '{newOrderGuid}',
		   '{item.Id}',
		   {item.Quantity}
		   )"));

                    // insert the items records.
                    command.CommandText = $@"INSERT INTO [dbo].[ordereditems]
           ([id]
           ,[orderid]
           ,[itemid]
           ,[quantity])
     VALUES" + valuesString;
                    await command.ExecuteNonQueryAsync();

                    // insert the shipping records.    
                    command.CommandText = $@"INSERT INTO [dbo].[ordershippingstatus]
           ([id]
           ,[orderid]
           ,[status]
           ,[shippingtype]
           ,[shippingaddress])
     VALUES
           (
               '{Guid.NewGuid()}',
               '{newOrderGuid}',
               'Pending processing',
               '{newOrder.ShippingInfo.ShippingType}',
               '{newOrder.ShippingInfo.ToString()}'
            )";
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = $@"INSERT INTO [dbo].[orders]
           ([id]
           ,[status]
           ,[customerid]
           ,[subtotal]
           ,[tax]
           ,[shippingcharges]
           ,[total]
           ,[createdate]
           ,[modifieddate])
VALUES (
	'{newOrderGuid}',
	'Pending processing',	
	'{newOrder.CustomerId}',
	{newOrder.SubTotal},
	{newOrder.Tax},
	{newOrder.ShippingCharges},
	{newOrder.Total},
	'{utcNow}',
	'{utcNow}'
)"; // insert the order records.
                    await command.ExecuteNonQueryAsync();

                    await sqlTran.CommitAsync();
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Failed to insert order records");
                    await sqlTran.RollbackAsync();
                }
            }

        }

        private bool ProcessPayment(CreateOrder newOrder)
        {
            var total = newOrder.Total;
            var payments = newOrder.Payments.Select(x => x.PaymentInfo).ToList();

            if (payments == null)//if no payment is submitted, we fetch the info from the db.

                payments = GetCustomerPaymentInfoFromDb(newOrder.CustomerId);

            PostTransaction(total, payments, DateTime.UtcNow);

            return true;
        }

        private List<PaymentInfo> GetCustomerPaymentInfoFromDb(Guid customerId)
        {
            return _dataAccess.FromTable("")
                .Query($@"
            SELECT
                pay.type, pay.paymentdetails
            FROM
                customers AS cust, payments AS pay 
            WHERE cust.id = payments.customerid
            AND cust.id = '{customerId}'")
            .Select(x =>
            {
                return new PaymentInfo()
                {
                    PaymentMethod = (PaymentMethod)(int)(x.type),
                    PInfo = JsonConvert.DeserializeObject((string)x.paymentdetails)
                };
            })
            .ToList();
        }

        private bool PostTransaction(float total, List<PaymentInfo> payments, DateTime transactionPostDate)
        {
            // maake call to a payment service.

            // insert records in the posted transactions tabel.

            return true;// assuming transactions always succeed.
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
        Task<bool> CancelOrder(Guid id);
    }
}