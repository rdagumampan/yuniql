CREATE VIEW [dbo].[VwVisitorTokenized]
AS
SELECT TOP (1000) [VisitorID]
      ,[FirstName] AS '${VwColumnPrefix1}FirstName'
      ,[LastName] AS '${VwColumnPrefix2}LastName'
      ,[Address] AS '${VwColumnPrefix3}Address'
      ,[Email] AS '${VwColumnPrefix4}Email'
FROM [dbo].[Visitor];
GO
