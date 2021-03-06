using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Models
{
    public class OrderCreated
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }

        public static OrderCreated FromDynamic(dynamic obj, ILogger logger)
        {
            try
            {
                return new OrderCreated()
                {
                    OrderId = (Guid)obj.id,
                    Status = (string)obj.status
                };
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.Message);
            }

            return null;
        }
    }

    public class CreateOrder
    {
        public Guid CustomerId { get; set; }
        public List<Item> Items { get; set; }
        public float SubTotal { get; set; }
        public float Tax { get; set; }
        public float ShippingCharges { get; set; }
        public float Total { get; set; }

        public List<OrderPaymentInfo> Payments { get; set; }

        public ShippingInfo ShippingInfo { get; set; }

        public OrderCreated ToCreatedOrder(Guid newId, string status)
        {
            return new OrderCreated
            {
                OrderId = newId,
                Status = status
            };
        }
    }

    public class Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
    }


    public class ShippingInfo
    {
        public ShippingType ShippingType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public override string ToString() => $"{AddressLine1}, {AddressLine2}, {City}, {State}, {Zip}";
    }

    public class PaymentInfo
    {
        public PaymentMethod PaymentMethod { get; set; }
        public object PInfo { get; set; }

    }
    public class OrderPaymentInfo
    {
        public PaymentInfo PaymentInfo { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public enum PaymentMethod
    {
        Credit,
        Debit,
        PayPal,
        Echeck
    }

    public enum ShippingType
    {
        InStore,
        Curb,
        Home,
        ThirdParty
    }

    public class DebitCardPayment : IPayment
    {

        public void ProcessTransaction()
        {

        }
    }

    public interface IPayment
    {
        void ProcessTransaction();
    }

    public class UpdateOrderStatus
    {
        public string Status { get; set; }
        public Guid OrderId { get; set; }
    }

    public class BulkCreateRequest
    {
        public List<CreateOrder> Items { get; set; }
    }

    public class BulkUpdateRequest
    {
        public List<UpdateOrderStatus> Items { get; set; }
    }

    public class BulkRequestResponse
    {
        public object Data { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMsg { get; set; }
        public BulkRequestResponse(Exception ex)
        {
            ErrorMsg = ex.Message;
            StatusCode = 500;
        }
        public BulkRequestResponse(object data, int statusCode)
        {
            Data = data;
            StatusCode = statusCode;
        }
    }

    public class Either<Fail, Success>
    {
        public Fail Left { get; private set; }
        public Success Right { get; private set; }

        public Either(Fail left)
        {
            Left = left;
        }

        public Either(Success right)
        {
            Right = right;
        }

        public T Match<T>(Func<Fail, T> ifLeft, Func<Success, T> ifRight)
        {
            if (Left == null && Right != null) return ifRight(Right);
            if (Right == null && Left != null) return ifLeft(Left);
            return default(T);
        }

        public bool IsRight => Right != null;

        public void IfRight(Action<Success> onRight) 
        {
            if(IsRight) onRight(Right);
        }
    }
}