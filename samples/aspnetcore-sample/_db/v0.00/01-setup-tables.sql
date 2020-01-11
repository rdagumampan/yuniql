CREATE TABLE [dbo].[Visitor](
	[VisitorID] [int] IDENTITY(1000,1) NOT NULL,
	[FirstName] [nvarchar](255) NULL,
	[LastName] [varchar](255) NULL,
	[Address] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL
) ON [PRIMARY];
GO
