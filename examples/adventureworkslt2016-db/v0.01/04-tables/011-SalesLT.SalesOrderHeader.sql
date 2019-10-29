SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [SalesLT].[SalesOrderHeader](
	[SalesOrderID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[RevisionNumber] [tinyint] NOT NULL,
	[OrderDate] [datetime] NOT NULL,
	[DueDate] [datetime] NOT NULL,
	[ShipDate] [datetime] NULL,
	[Status] [tinyint] NOT NULL,
	[OnlineOrderFlag] [dbo].[Flag] NOT NULL,
	[SalesOrderNumber]  AS (isnull(N'SO'+CONVERT([nvarchar](23),[SalesOrderID]),N'*** ERROR ***')),
	[PurchaseOrderNumber] [dbo].[OrderNumber] NULL,
	[AccountNumber] [dbo].[AccountNumber] NULL,
	[CustomerID] [int] NOT NULL,
	[ShipToAddressID] [int] NULL,
	[BillToAddressID] [int] NULL,
	[ShipMethod] [nvarchar](50) NOT NULL,
	[CreditCardApprovalCode] [varchar](15) NULL,
	[SubTotal] [money] NOT NULL,
	[TaxAmt] [money] NOT NULL,
	[Freight] [money] NOT NULL,
	[TotalDue]  AS (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))),
	[Comment] [nvarchar](max) NULL,
	[rowguid] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SalesOrderHeader_SalesOrderID] PRIMARY KEY CLUSTERED 
(
	[SalesOrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [AK_SalesOrderHeader_rowguid] UNIQUE NONCLUSTERED 
(
	[rowguid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [AK_SalesOrderHeader_SalesOrderNumber] UNIQUE NONCLUSTERED 
(
	[SalesOrderNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_SalesOrderHeader_CustomerID] ON [SalesLT].[SalesOrderHeader]
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ADD  CONSTRAINT [DF_SalesOrderHeader_RevisionNumber]  DEFAULT ((0)) FOR [RevisionNumber]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ADD  CONSTRAINT [DF_SalesOrderHeader_OrderDate]  DEFAULT (getdate()) FOR [OrderDate]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ADD  CONSTRAINT [DF_SalesOrderHeader_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ADD  CONSTRAINT [DF_SalesOrderHeader_OnlineOrderFlag]  DEFAULT ((1)) FOR [OnlineOrderFlag]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ADD  CONSTRAINT [DF_SalesOrderHeader_SubTotal]  DEFAULT ((0.00)) FOR [SubTotal]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ADD  CONSTRAINT [DF_SalesOrderHeader_TaxAmt]  DEFAULT ((0.00)) FOR [TaxAmt]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ADD  CONSTRAINT [DF_SalesOrderHeader_Freight]  DEFAULT ((0.00)) FOR [Freight]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ADD  CONSTRAINT [DF_SalesOrderHeader_rowguid]  DEFAULT (newid()) FOR [rowguid]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ADD  CONSTRAINT [DF_SalesOrderHeader_ModifiedDate]  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader]  WITH NOCHECK ADD  CONSTRAINT [FK_SalesOrderHeader_Address_BillTo_AddressID] FOREIGN KEY([BillToAddressID])
REFERENCES [SalesLT].[Address] ([AddressID])
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] CHECK CONSTRAINT [FK_SalesOrderHeader_Address_BillTo_AddressID]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader]  WITH NOCHECK ADD  CONSTRAINT [FK_SalesOrderHeader_Address_ShipTo_AddressID] FOREIGN KEY([ShipToAddressID])
REFERENCES [SalesLT].[Address] ([AddressID])
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] CHECK CONSTRAINT [FK_SalesOrderHeader_Address_ShipTo_AddressID]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader]  WITH NOCHECK ADD  CONSTRAINT [FK_SalesOrderHeader_Customer_CustomerID] FOREIGN KEY([CustomerID])
REFERENCES [SalesLT].[Customer] ([CustomerID])
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] CHECK CONSTRAINT [FK_SalesOrderHeader_Customer_CustomerID]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader]  WITH NOCHECK ADD  CONSTRAINT [CK_SalesOrderHeader_DueDate] CHECK  (([DueDate]>=[OrderDate]))
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] CHECK CONSTRAINT [CK_SalesOrderHeader_DueDate]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader]  WITH NOCHECK ADD  CONSTRAINT [CK_SalesOrderHeader_Freight] CHECK  (([Freight]>=(0.00)))
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] CHECK CONSTRAINT [CK_SalesOrderHeader_Freight]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader]  WITH NOCHECK ADD  CONSTRAINT [CK_SalesOrderHeader_ShipDate] CHECK  (([ShipDate]>=[OrderDate] OR [ShipDate] IS NULL))
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] CHECK CONSTRAINT [CK_SalesOrderHeader_ShipDate]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader]  WITH NOCHECK ADD  CONSTRAINT [CK_SalesOrderHeader_Status] CHECK  (([Status]>=(0) AND [Status]<=(8)))
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] CHECK CONSTRAINT [CK_SalesOrderHeader_Status]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader]  WITH NOCHECK ADD  CONSTRAINT [CK_SalesOrderHeader_SubTotal] CHECK  (([SubTotal]>=(0.00)))
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] CHECK CONSTRAINT [CK_SalesOrderHeader_SubTotal]
GO
ALTER TABLE [SalesLT].[SalesOrderHeader]  WITH NOCHECK ADD  CONSTRAINT [CK_SalesOrderHeader_TaxAmt] CHECK  (([TaxAmt]>=(0.00)))
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] CHECK CONSTRAINT [CK_SalesOrderHeader_TaxAmt]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [SalesLT].[uSalesOrderHeader] ON [SalesLT].[SalesOrderHeader] 
AFTER UPDATE AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        -- Update RevisionNumber for modification of any field EXCEPT the Status.
        IF NOT (UPDATE([Status]) OR UPDATE([RevisionNumber]))
        BEGIN
            UPDATE [SalesLT].[SalesOrderHeader]
            SET [SalesLT].[SalesOrderHeader].[RevisionNumber] = 
                [SalesLT].[SalesOrderHeader].[RevisionNumber] + 1
            WHERE [SalesLT].[SalesOrderHeader].[SalesOrderID] IN 
                (SELECT inserted.[SalesOrderID] FROM inserted);
        END;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
GO
ALTER TABLE [SalesLT].[SalesOrderHeader] ENABLE TRIGGER [uSalesOrderHeader]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Primary key.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'SalesOrderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Incremental number to track changes to the sales order over time.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of 0' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderHeader_RevisionNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Dates the sales order was created.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'OrderDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of GETDATE()' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderHeader_OrderDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date the order is due to the customer.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'DueDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date the order was shipped to the customer.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'ShipDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Order current status. 1 = In process; 2 = Approved; 3 = Backordered; 4 = Rejected; 5 = Shipped; 6 = Cancelled' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of 1' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderHeader_Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 = Order placed by sales person. 1 = Order placed online by customer.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'OnlineOrderFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of 1 (TRUE)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderHeader_OnlineOrderFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique sales order identification number.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'SalesOrderNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Customer purchase order number reference. ' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'PurchaseOrderNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Financial accounting number reference.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'AccountNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Customer identification number. Foreign key to Customer.CustomerID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'CustomerID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The ID of the location to send goods.  Foreign key to the Address table.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'ShipToAddressID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The ID of the location to send invoices.  Foreign key to the Address table.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'BillToAddressID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Shipping method. Foreign key to ShipMethod.ShipMethodID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'ShipMethod'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Approval code provided by the credit card company.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'CreditCardApprovalCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sales subtotal. Computed as SUM(SalesOrderDetail.LineTotal)for the appropriate SalesOrderID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'SubTotal'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of 0.0' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderHeader_SubTotal'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tax amount.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'TaxAmt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of 0.0' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderHeader_TaxAmt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Shipping cost.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'Freight'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of 0.0' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderHeader_Freight'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Total due from customer. Computed as Subtotal + TaxAmt + Freight.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'TotalDue'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sales representative comments.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'Comment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'rowguid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of NEWID()' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderHeader_rowguid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date and time the record was last updated.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'COLUMN',@level2name=N'ModifiedDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of GETDATE()' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderHeader_ModifiedDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Primary key (clustered) constraint' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'PK_SalesOrderHeader_SalesOrderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique nonclustered constraint. Used to support replication samples.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'AK_SalesOrderHeader_rowguid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique nonclustered constraint.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'AK_SalesOrderHeader_SalesOrderNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nonclustered index.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'INDEX',@level2name=N'IX_SalesOrderHeader_CustomerID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'General sales order information.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AFTER UPDATE trigger that updates the RevisionNumber and ModifiedDate columns in the SalesOrderHeader table.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'TRIGGER',@level2name=N'uSalesOrderHeader'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Foreign key constraint referencing Address.AddressID for BillTo.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'FK_SalesOrderHeader_Address_BillTo_AddressID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Foreign key constraint referencing Address.AddressID for ShipTo.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'FK_SalesOrderHeader_Address_ShipTo_AddressID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Foreign key constraint referencing Customer.CustomerID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'FK_SalesOrderHeader_Customer_CustomerID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [DueDate] >= [OrderDate]' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'CK_SalesOrderHeader_DueDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [Freight] >= (0.00)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'CK_SalesOrderHeader_Freight'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [ShipDate] >= [OrderDate] OR [ShipDate] IS NULL' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'CK_SalesOrderHeader_ShipDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [Status] BETWEEN (0) AND (8)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'CK_SalesOrderHeader_Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [SubTotal] >= (0.00)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'CK_SalesOrderHeader_SubTotal'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [TaxAmt] >= (0.00)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderHeader', @level2type=N'CONSTRAINT',@level2name=N'CK_SalesOrderHeader_TaxAmt'
GO
