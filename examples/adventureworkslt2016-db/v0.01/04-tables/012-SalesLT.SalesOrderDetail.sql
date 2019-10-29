SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [SalesLT].[SalesOrderDetail](
	[SalesOrderID] [int] NOT NULL,
	[SalesOrderDetailID] [int] IDENTITY(1,1) NOT NULL,
	[OrderQty] [smallint] NOT NULL,
	[ProductID] [int] NOT NULL,
	[UnitPrice] [money] NOT NULL,
	[UnitPriceDiscount] [money] NOT NULL,
	[LineTotal]  AS (isnull(([UnitPrice]*((1.0)-[UnitPriceDiscount]))*[OrderQty],(0.0))),
	[rowguid] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID] PRIMARY KEY CLUSTERED 
(
	[SalesOrderID] ASC,
	[SalesOrderDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [AK_SalesOrderDetail_rowguid] UNIQUE NONCLUSTERED 
(
	[rowguid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
GO
CREATE NONCLUSTERED INDEX [IX_SalesOrderDetail_ProductID] ON [SalesLT].[SalesOrderDetail]
(
	[ProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
ALTER TABLE [SalesLT].[SalesOrderDetail] ADD  CONSTRAINT [DF_SalesOrderDetail_UnitPriceDiscount]  DEFAULT ((0.0)) FOR [UnitPriceDiscount]
GO
ALTER TABLE [SalesLT].[SalesOrderDetail] ADD  CONSTRAINT [DF_SalesOrderDetail_rowguid]  DEFAULT (newid()) FOR [rowguid]
GO
ALTER TABLE [SalesLT].[SalesOrderDetail] ADD  CONSTRAINT [DF_SalesOrderDetail_ModifiedDate]  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [SalesLT].[SalesOrderDetail]  WITH NOCHECK ADD  CONSTRAINT [FK_SalesOrderDetail_Product_ProductID] FOREIGN KEY([ProductID])
REFERENCES [SalesLT].[Product] ([ProductID])
GO
ALTER TABLE [SalesLT].[SalesOrderDetail] CHECK CONSTRAINT [FK_SalesOrderDetail_Product_ProductID]
GO
ALTER TABLE [SalesLT].[SalesOrderDetail]  WITH NOCHECK ADD  CONSTRAINT [FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID] FOREIGN KEY([SalesOrderID])
REFERENCES [SalesLT].[SalesOrderHeader] ([SalesOrderID])
ON DELETE CASCADE
GO
ALTER TABLE [SalesLT].[SalesOrderDetail] CHECK CONSTRAINT [FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID]
GO
ALTER TABLE [SalesLT].[SalesOrderDetail]  WITH NOCHECK ADD  CONSTRAINT [CK_SalesOrderDetail_OrderQty] CHECK  (([OrderQty]>(0)))
GO
ALTER TABLE [SalesLT].[SalesOrderDetail] CHECK CONSTRAINT [CK_SalesOrderDetail_OrderQty]
GO
ALTER TABLE [SalesLT].[SalesOrderDetail]  WITH NOCHECK ADD  CONSTRAINT [CK_SalesOrderDetail_UnitPrice] CHECK  (([UnitPrice]>=(0.00)))
GO
ALTER TABLE [SalesLT].[SalesOrderDetail] CHECK CONSTRAINT [CK_SalesOrderDetail_UnitPrice]
GO
ALTER TABLE [SalesLT].[SalesOrderDetail]  WITH NOCHECK ADD  CONSTRAINT [CK_SalesOrderDetail_UnitPriceDiscount] CHECK  (([UnitPriceDiscount]>=(0.00)))
GO
ALTER TABLE [SalesLT].[SalesOrderDetail] CHECK CONSTRAINT [CK_SalesOrderDetail_UnitPriceDiscount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [SalesLT].[iduSalesOrderDetail] ON [SalesLT].[SalesOrderDetail] 
AFTER INSERT, DELETE, UPDATE AS 
BEGIN
    DECLARE @Count int;

    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        -- If inserting or updating these columns
        IF UPDATE([ProductID]) OR UPDATE([OrderQty]) OR UPDATE([UnitPrice]) OR UPDATE([UnitPriceDiscount]) 

        -- Update SubTotal in SalesOrderHeader record. Note that this causes the 
        -- SalesOrderHeader trigger to fire which will update the RevisionNumber.
        UPDATE [SalesLT].[SalesOrderHeader]
        SET [SalesLT].[SalesOrderHeader].[SubTotal] = 
            (SELECT SUM([SalesLT].[SalesOrderDetail].[LineTotal])
                FROM [SalesLT].[SalesOrderDetail]
                WHERE [SalesLT].[SalesOrderHeader].[SalesOrderID] = [SalesLT].[SalesOrderDetail].[SalesOrderID])
        WHERE [SalesLT].[SalesOrderHeader].[SalesOrderID] IN (SELECT inserted.[SalesOrderID] FROM inserted);

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
ALTER TABLE [SalesLT].[SalesOrderDetail] ENABLE TRIGGER [iduSalesOrderDetail]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Primary key. Foreign key to SalesOrderHeader.SalesOrderID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'COLUMN',@level2name=N'SalesOrderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Primary key. One incremental unique number per product sold.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'COLUMN',@level2name=N'SalesOrderDetailID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Quantity ordered per product.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'COLUMN',@level2name=N'OrderQty'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Product sold to customer. Foreign key to Product.ProductID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'COLUMN',@level2name=N'ProductID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Selling price of a single product.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'COLUMN',@level2name=N'UnitPrice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Discount amount.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'COLUMN',@level2name=N'UnitPriceDiscount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of 0.0' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderDetail_UnitPriceDiscount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'COLUMN',@level2name=N'LineTotal'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'COLUMN',@level2name=N'rowguid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of NEWID()' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderDetail_rowguid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date and time the record was last updated.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'COLUMN',@level2name=N'ModifiedDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of GETDATE()' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'CONSTRAINT',@level2name=N'DF_SalesOrderDetail_ModifiedDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Primary key (clustered) constraint' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'CONSTRAINT',@level2name=N'PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique nonclustered constraint. Used to support replication samples.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'CONSTRAINT',@level2name=N'AK_SalesOrderDetail_rowguid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nonclustered index.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'INDEX',@level2name=N'IX_SalesOrderDetail_ProductID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Individual products associated with a specific sales order. See SalesOrderHeader.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Foreign key constraint referencing SalesOrderHeader.SalesOrderID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'CONSTRAINT',@level2name=N'FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [OrderQty] > (0)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'CONSTRAINT',@level2name=N'CK_SalesOrderDetail_OrderQty'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [UnitPrice] >= (0.00)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'CONSTRAINT',@level2name=N'CK_SalesOrderDetail_UnitPrice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [UnitPriceDiscount] >= (0.00)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'SalesOrderDetail', @level2type=N'CONSTRAINT',@level2name=N'CK_SalesOrderDetail_UnitPriceDiscount'
GO
