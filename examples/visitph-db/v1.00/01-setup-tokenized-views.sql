CREATE VIEW [dbo].[VwVisitorTokenized]
AS
SELECT TOP (1000) [VisitorID]
      ,[FirstName] AS '${VwColumnPrefix}FirstName'
      ,[LastName] AS '${VwColumnPrefix}LastName'
      ,[Address] AS '${VwColumnPrefix}Address'
      ,[Email] AS '${VwColumnPrefix}Email'
FROM [dbo].[Visitor];
GO
