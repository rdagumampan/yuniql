CREATE VIEW [dbo].[VwVisitor]
AS
SELECT TOP (1000) [VisitorID]
      ,[FirstName]
      ,[LastName]
      ,[Address]
      ,[Email]
FROM [dbo].[Visitor];
GO