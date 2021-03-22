using System;
using Common.Services;
using Moq;
using Xunit;
using bulk_order_api.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Api.Tests
{


    public class BulkApiTests
    {
        class Mocker
        {
            public Mock<ILogger<BulkOrdersController>> MockLogger { get; set; }
            public Mock<IBulkOrderService> MockBulkOrderService { get; set; }
            public Mock<ICreateOrderPublisher> MockCreateOrderPublisher { get; set; }


            public Mocker()
            {
                MockLogger = new Mock<ILogger<BulkOrdersController>>();
                MockBulkOrderService = new Mock<IBulkOrderService>();
                MockCreateOrderPublisher = new Mock<ICreateOrderPublisher>();
            }

            public BulkOrdersController Build()
            {
                return new BulkOrdersController(MockLogger.Object, MockBulkOrderService.Object);
            }
        }

        [Fact]
        public async void create_bulk_orders_endpoint_returns_bad_request_for_bad_post_request_body()
        {
            var mock = new Mocker();
            var sut = mock.Build();
            var res = await sut.CreateBulkOrders(null);
            Assert.IsType<BadRequestResult>(res);

            mock.MockBulkOrderService.Verify(x => x.BulkCreateOrders(It.IsAny<CreateOrder[]>()), Times.Never());
        }

        [Fact]
        public async void create_bulk_order_endpoint_returns_ok_if_post_request_body_is_properly_formed()
        {
            var mock = new Mocker();
            var sut = mock.Build();

            var res = await sut.CreateBulkOrders(new BulkCreateRequest()
            {
                Items = new System.Collections.Generic.List<CreateOrder>()
                {
                    new CreateOrder()
                    {

                    }
                }
            });
            Assert.IsType<OkObjectResult>(res);

            mock.MockBulkOrderService.Verify(x => x.BulkCreateOrders(It.IsAny<CreateOrder[]>()), Times.Once());
        }

        [Fact]
        public async void update_bulk_orders_endpoint_returns_bad_request_for_bad_post_request_body()
        {
            var mock = new Mocker();
            var sut = mock.Build();
            var res = await sut.UpdateBulkOrders(null);
            Assert.IsType<BadRequestResult>(res);

            mock.MockBulkOrderService.Verify(x => x.BulkUpdateOrders(It.IsAny<UpdateOrderStatus[]>()), Times.Never());
        }

        [Fact]
        public async void update_bulk_order_endpoint_returns_ok_if_post_request_body_is_properly_formed()
        {
            var mock = new Mocker();
            var sut = mock.Build();

            var res = await sut.UpdateBulkOrders(new BulkUpdateRequest()
            {

                Items = new System.Collections.Generic.List<UpdateOrderStatus>()
                {
                    new UpdateOrderStatus()
                    {

                    }
                }
            });
            Assert.IsType<OkObjectResult>(res);
            mock.MockBulkOrderService.Verify(x => x.BulkUpdateOrders(It.IsAny<UpdateOrderStatus[]>()), Times.Once());
        }

        [Fact]
        public async void create_bulk_order_returns_appropriate_status_codes_for_individual_requests()
        {
            var mock = new Mocker();
            var sut = mock.Build();

            mock.MockBulkOrderService
                .Setup(x => x.BulkCreateOrders(It.IsAny<CreateOrder[]>()))
                .ReturnsAsync(new List<Either<Exception, bool>>()
                {
                    new Either<Exception, bool>(true), // true indicates we accepted, otherwise we rejected to accept this request for further processing.
                    new Either<Exception, bool>(false),
                }.ToArray());

            var res = await sut.CreateBulkOrders(new BulkCreateRequest()
            {
                Items = new System.Collections.Generic.List<CreateOrder>()
                {
                    new CreateOrder()
                    {

                    },
                    new CreateOrder()
                    {

                    }
                }
            });
            Assert.IsType<OkObjectResult>(res);

            mock.MockBulkOrderService.Verify(x => x.BulkCreateOrders(It.IsAny<CreateOrder[]>()), Times.Once());

            var value = (res as OkObjectResult).Value as List<BulkRequestResponse>;
            Assert.Equal((int)HttpStatusCode.Accepted, value.FirstOrDefault().StatusCode);
            Assert.Equal((int)HttpStatusCode.UnprocessableEntity, value.LastOrDefault().StatusCode);
        }

    }
}
