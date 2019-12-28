CREATE PROCEDURE [dbo].[usp_insert_visitor]  (
 @firstName NVARCHAR(255),
 @lastName NVARCHAR(255),
 @address NVARCHAR(255),
 @email NVARCHAR(255))
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO [dbo].[Visitor] (FirstName, LastName, Address, Email)
	VALUES (@firstName, @lastName, @address, @email);

	SELECT @@IDENTITY;
END;
GO

CREATE PROCEDURE [dbo].[usp_update_visitor]  (
 @visitorID INT,
 @firstName NVARCHAR(255),
 @lastName NVARCHAR(255),
 @address NVARCHAR(255),
 @email NVARCHAR(255))
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE [dbo].Visitor
	SET
		FirstName = @firstName,
		LastName = @lastName,
		Address = @address,
		Email = @email
	WHERE
		VisitorID = @visitorID;
END;
GO

CREATE PROCEDURE [dbo].[usp_delete_visitor]  (
 @visitorID INT
) AS
BEGIN
	SET NOCOUNT ON;
	
	DELETE FROM [dbo].[Visitor]
	WHERE
		VisitorID = @visitorID;
END;
GO


