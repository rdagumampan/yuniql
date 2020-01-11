IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Destination'))
	DROP TABLE [dbo].[Destination];
GO

CREATE TABLE [dbo].[Destination](
	[DestinationID] [int] IDENTITY(1000,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Address] [nvarchar](255) NOT NULL,
	[Telephone] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](255) NOT NULL
) ON [PRIMARY];
GO