use master 
go

alter database ecomm_db set single_user with rollback immediate 
go

drop DATABASE ecomm_db 
go 

create database ecomm_db 
go 

USE [ecomm_db]
GO

PRINT 'CREATING CUSTOMERS'
CREATE TABLE [dbo].[customers](
	[id] [uniqueidentifier] NULL,
	[fname] [nvarchar](50) NULL,
	[lname] [nvarchar](50) NULL,
	[gender] [char](1) NULL,
	[dob] [date] NULL,
	[address] [nvarchar](100) NULL,
	[zip] [nvarchar](5) NULL,
	[city] [nvarchar](30) NULL
) ON [PRIMARY]
GO

PRINT 'CREATING ORDERS'
CREATE TABLE [dbo].[orders](
	[id] [uniqueidentifier] PRIMARY KEY,
	[status] [nvarchar](50) NOT NULL,
	[customerid] [uniqueidentifier] NOT NULL,
	[subtotal] [smallmoney] NOT NULL,
	[tax] [smallmoney] 	NOT NULL,
	[shippingcharges] [smallmoney] 	NOT NULL,
	[total] [smallmoney] 	NOT NULL,
	[createdate] [datetime] DEFAULT GETDATE() NOT NULL,
	[modifieddate] [datetime] DEFAULT GETDATE() NOT NULL
) ON [PRIMARY]
GO

PRINT 'CREATING PAYMENTS'
CREATE TABLE [dbo].[payments](
	[id] [uniqueidentifier] PRIMARY KEY,
	[type] [int]  NOT NULL,
	[customerid] [uniqueidentifier] NOT NULL,
	[paymentdetails] [nvarchar](max) not null
) ON [PRIMARY]
GO

 PRINT 'CREATING PENDING TRANSACTIONS'
 CREATE TABLE [dbo].[pendingtransactions](
 	[id] [uniqueidentifier] PRIMARY KEY,
	[customerid] [uniqueidentifier] NOT NULL,
	[orderid] [uniqueidentifier] NOT NULL,
	[paymentid] [uniqueidentifier] NOT NULL
 ) ON [PRIMARY]
 GO

PRINT 'CREATING ORDERSHIPPING'
CREATE TABLE [dbo].[ordershippingstatus](
	[id] [uniqueidentifier] PRIMARY KEY,
	[orderid] [uniqueidentifier]  NOT NULL,
	[status] [nvarchar](200) NOT NULL,
	[shippingtype] [nvarchar](100)  NOT NULL,
	[shippingaddress] [nvarchar](300) NOT NULL
) ON [PRIMARY]
GO
PRINT 'CREATING ITEMS'
CREATE TABLE [dbo].[items](
	[id] [uniqueidentifier] PRIMARY KEY,
	[name] [nvarchar] (200) NOT NULL,
	[price] [smallmoney]  NOT NULL,
	[stock] [int] NOT NULL
) ON [PRIMARY]
GO
PRINT 'CREATING ORDEREDITEMS'
CREATE TABLE [dbo].[ordereditems](
	[id] [uniqueidentifier] PRIMARY KEY,
	[orderid] [uniqueidentifier]  NOT NULL,
	[itemid] [uniqueidentifier]  NOT NULL,
	[quantity] [int] NOT NULL
) ON [PRIMARY]
GO
