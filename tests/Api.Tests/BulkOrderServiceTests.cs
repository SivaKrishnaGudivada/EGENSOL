using System;
using Common.Services;
using Moq;
using Xunit;
using bulk_order_api.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Models;
using Common.DataAccess;
using System.Collections.Generic;

namespace Api.Tests
{


    public class BulkOrderServiceTests
    {
        class Mocker
        {

            public Mock<IMassiveOrmDataAccess> MockDataAccess { get; set; }
            public Mock<IOrderService> MockOrderService { get; set; }
            public Mock<ILogger<BulkOrderService>> MockLogger { get; set; }
            public Mock<ICreateOrderPublisher> MockCreateOrderPublisher { get; set; }


            public Mocker()
            {
                MockDataAccess = new Mock<IMassiveOrmDataAccess>();
                MockOrderService = new Mock<IOrderService>();
                MockLogger = new Mock<ILogger<BulkOrderService>>();
                MockCreateOrderPublisher = new Mock<ICreateOrderPublisher>();
            }

            public BulkOrderService Build()
            {
                return new BulkOrderService(MockDataAccess.Object, MockOrderService.Object, MockLogger.Object, MockCreateOrderPublisher.Object);
            }
        }
        
        [Fact]
        public void bulk_order_create_throws_exc_when_args_are_null()
        {
            var mock = new Mocker();
            var sut = mock.Build();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.BulkCreateOrders(null));
            mock.MockCreateOrderPublisher.Verify(x => x.Publish(It.IsAny<CreateOrder>()), Times.Never());
        }

        [Fact]
        public void bulk_order_update_throws_exc_when_args_are_null()
        {
            var mock = new Mocker();
            var sut = mock.Build();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.BulkUpdateOrders(null));
            mock.MockCreateOrderPublisher.Verify(x => x.Publish(It.IsAny<CreateOrder>()), Times.Never());
        }

        [Fact]
        public async void bulk_order_service_queues_order_for_creation_if_it_can_be_accepted()
        {
            var mock = new Mocker();
            mock.MockOrderService.Setup(x=>x.CanAcceptOrder(It.IsAny<CreateOrder>())).ReturnsAsync(true);
            var sut = mock.Build();

            var res = await sut.BulkCreateOrders(new List<CreateOrder>() 
            {
                new CreateOrder()
                {

                }
            }.ToArray());

            mock.MockCreateOrderPublisher.Verify(x => x.Publish(It.IsAny<CreateOrder>()), Times.Once());
        }

        [Fact]
        public async void bulk_order_service_does_not_queue_order_for_creation_if_it_cannot_be_accepted()
        {
            var mock = new Mocker();
            mock.MockOrderService.Setup(x=>x.CanAcceptOrder(It.IsAny<CreateOrder>())).ReturnsAsync(false);
            var sut = mock.Build();

            var res = await sut.BulkCreateOrders(new List<CreateOrder>() 
            {
                new CreateOrder()
                {
                    
                }
            }.ToArray());

            mock.MockCreateOrderPublisher.Verify(x => x.Publish(It.IsAny<CreateOrder>()), Times.Never());
        }
    }
}
