SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [SalesLT].[Product](
	[ProductID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [dbo].[Name] NOT NULL,
	[ProductNumber] [nvarchar](25) NOT NULL,
	[Color] [nvarchar](15) NULL,
	[StandardCost] [money] NOT NULL,
	[ListPrice] [money] NOT NULL,
	[Size] [nvarchar](5) NULL,
	[Weight] [decimal](8, 2) NULL,
	[ProductCategoryID] [int] NULL,
	[ProductModelID] [int] NULL,
	[SellStartDate] [datetime] NOT NULL,
	[SellEndDate] [datetime] NULL,
	[DiscontinuedDate] [datetime] NULL,
	[ThumbNailPhoto] [varbinary](max) NULL,
	[ThumbnailPhotoFileName] [nvarchar](50) NULL,
	[rowguid] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Product_ProductID] PRIMARY KEY CLUSTERED 
(
	[ProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [AK_Product_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [AK_Product_ProductNumber] UNIQUE NONCLUSTERED 
(
	[ProductNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [AK_Product_rowguid] UNIQUE NONCLUSTERED 
(
	[rowguid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
GO
SET ANSI_PADDING ON
GO
ALTER TABLE [SalesLT].[Product] ADD  CONSTRAINT [DF_Product_rowguid]  DEFAULT (newid()) FOR [rowguid]
GO
ALTER TABLE [SalesLT].[Product] ADD  CONSTRAINT [DF_Product_ModifiedDate]  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [SalesLT].[Product]  WITH NOCHECK ADD  CONSTRAINT [FK_Product_ProductCategory_ProductCategoryID] FOREIGN KEY([ProductCategoryID])
REFERENCES [SalesLT].[ProductCategory] ([ProductCategoryID])
GO
ALTER TABLE [SalesLT].[Product] CHECK CONSTRAINT [FK_Product_ProductCategory_ProductCategoryID]
GO
ALTER TABLE [SalesLT].[Product]  WITH NOCHECK ADD  CONSTRAINT [FK_Product_ProductModel_ProductModelID] FOREIGN KEY([ProductModelID])
REFERENCES [SalesLT].[ProductModel] ([ProductModelID])
GO
ALTER TABLE [SalesLT].[Product] CHECK CONSTRAINT [FK_Product_ProductModel_ProductModelID]
GO
ALTER TABLE [SalesLT].[Product]  WITH NOCHECK ADD  CONSTRAINT [CK_Product_ListPrice] CHECK  (([ListPrice]>=(0.00)))
GO
ALTER TABLE [SalesLT].[Product] CHECK CONSTRAINT [CK_Product_ListPrice]
GO
ALTER TABLE [SalesLT].[Product]  WITH NOCHECK ADD  CONSTRAINT [CK_Product_SellEndDate] CHECK  (([SellEndDate]>=[SellStartDate] OR [SellEndDate] IS NULL))
GO
ALTER TABLE [SalesLT].[Product] CHECK CONSTRAINT [CK_Product_SellEndDate]
GO
ALTER TABLE [SalesLT].[Product]  WITH NOCHECK ADD  CONSTRAINT [CK_Product_StandardCost] CHECK  (([StandardCost]>=(0.00)))
GO
ALTER TABLE [SalesLT].[Product] CHECK CONSTRAINT [CK_Product_StandardCost]
GO
ALTER TABLE [SalesLT].[Product]  WITH NOCHECK ADD  CONSTRAINT [CK_Product_Weight] CHECK  (([Weight]>(0.00)))
GO
ALTER TABLE [SalesLT].[Product] CHECK CONSTRAINT [CK_Product_Weight]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Primary key for Product records.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'ProductID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of the product.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique product identification number.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'ProductNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Product color.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'Color'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Standard cost of the product.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'StandardCost'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Selling price.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'ListPrice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Product size.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'Size'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Product weight.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'Weight'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Product is a member of this product category. Foreign key to ProductCategory.ProductCategoryID. ' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'ProductCategoryID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Product is a member of this product model. Foreign key to ProductModel.ProductModelID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'ProductModelID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date the product was available for sale.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'SellStartDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date the product was no longer available for sale.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'SellEndDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date the product was discontinued.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'DiscontinuedDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Small image of the product.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'ThumbNailPhoto'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Small image file name.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'ThumbnailPhotoFileName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'rowguid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of NEWID()' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'DF_Product_rowguid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date and time the record was last updated.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'COLUMN',@level2name=N'ModifiedDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Default constraint value of GETDATE()' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'DF_Product_ModifiedDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Primary key (clustered) constraint' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'PK_Product_ProductID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique nonclustered constraint.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'AK_Product_Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique nonclustered constraint.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'AK_Product_ProductNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique nonclustered constraint. Used to support replication samples.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'AK_Product_rowguid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Products sold or used in the manfacturing of sold products.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Foreign key constraint referencing ProductCategory.ProductCategoryID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'FK_Product_ProductCategory_ProductCategoryID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Foreign key constraint referencing ProductModel.ProductModelID.' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'FK_Product_ProductModel_ProductModelID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [ListPrice] >= (0.00)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'CK_Product_ListPrice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [SellEndDate] >= [SellStartDate] OR [SellEndDate] IS NULL' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'CK_Product_SellEndDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Check constraint [Weight] > (0.00)' , @level0type=N'SCHEMA',@level0name=N'SalesLT', @level1type=N'TABLE',@level1name=N'Product', @level2type=N'CONSTRAINT',@level2name=N'CK_Product_Weight'
GO
