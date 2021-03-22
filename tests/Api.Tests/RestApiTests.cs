using System;
using Common.Services;
using Moq;
using Xunit;
using egen_sol.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Api.Tests
{
    public class RestApiTests
    {

    class Mocker
    {
        public Mock<ILogger<OrdersController>> MockLogger { get; set; }
        public Mock<IOrderService> MockOrderService { get; set; }
        public Mock<ICreateOrderPublisher> MockCreateOrderPublisher { get; set; }

        public Mocker()
        {
            MockLogger = new Mock<ILogger<OrdersController>>();
            MockOrderService = new Mock<IOrderService>();
            MockCreateOrderPublisher = new Mock<ICreateOrderPublisher>();
        }

        public OrdersController Build()
        {
            return new OrdersController(MockLogger.Object, MockOrderService.Object, MockCreateOrderPublisher.Object);
        }
    }

        [Fact]
        public async void order_create_endpoint_returns_bad_request_for_bad_post_request_body()
        {
            var sut = new Mocker().Build();
            var res = await sut.Create(null);
            Assert.IsType<BadRequestResult>(res);
        }

        [Fact]
        public async void order_create_endpoint_returns_request_accepted_if_it_can_be_processed()
        {
            var mock = new Mocker();
            mock.MockOrderService.Setup(x => x.CanAcceptOrder(It.IsAny<CreateOrder>())).ReturnsAsync(true);
            var sut = mock.Build();

            var res = await sut.Create(new CreateOrder() { });
            Assert.IsType<AcceptedResult>(res);
        }

        [Fact]
        public async void order_create_endpoint_returns_request_accepted_if_it_cannot_be_processed()
        {
            var mock = new Mocker();
            mock.MockOrderService.Setup(x => x.CanAcceptOrder(It.IsAny<CreateOrder>())).ReturnsAsync(false);
            var sut = mock.Build();

            var res = await sut.Create(new CreateOrder() { });
            Assert.IsType<UnprocessableEntityResult>(res);
        }

        [Fact]
        public async void cancel_order_endpoint_returns_nocontent_if_cancellation_succeeds()
        {
            var mock = new Mocker();
            var sut = mock.Build();

            var res = await sut.CancelOrder(Guid.NewGuid());

            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public void get_orders_calls_order_service()
        {
            var mock = new Mocker();
            var sut = mock.Build();
            var res = sut.GetAllOrders();

            mock.MockOrderService.Verify(x => x.GetOrders(), Times.Once());
        }

        [Fact]
        public void get_order_by_id_calls_order_service()
        {
            var mock = new Mocker();
            var sut = mock.Build();
            var res = sut.GetOrderById(Guid.NewGuid());

            mock.MockOrderService.Verify(x => x.GetOrderById(It.IsAny<Guid>()), Times.Once());
        }
    }
}