
# Database migrations

This database migration project is created and to be executed thru `yuniql` tool. 
For documentation and how-to guides, please visit yuniql [github page](https://github.com/rdagumampan/yuniql).

### Clean up example db
```sql
DROP TABLE [dbo].[__YuniqlDbVersion];
DROP VIEW [dbo].[VwVisitor];
DROP VIEW [dbo].[VwVisitorTokenized];
DROP PROC [dbo].[usp_delete_visitor];
DROP PROC [dbo].[usp_insert_visitor];
DROP PROC [dbo].[usp_update_visitor];
DROP TABLE [dbo].[Destination];
DROP TABLE [dbo].[Visitor];
DROP FUNCTION [dbo].[udf_calculate_visitors];
```