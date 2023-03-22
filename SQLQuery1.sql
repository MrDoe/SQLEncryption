Use ClassLibrarySQL
GO

Create Assembly ClassLibrarySQL from 'F:\My Development\ClassLibrarySQL\ClassLibrarySQL\bin\Debug\ClassLibrarySQL.dll' 
GO

CREATE FUNCTION AddGstTax(@inputOne float)
RETURNS [float] WITH EXECUTE AS CALLER, RETURNS NULL ON NULL INPUT
AS 
EXTERNAL NAME [ClassLibrarySQL].[ClassLibrarySQL.Class1].[AddTax]
GO

EXEC sp_configure 'show advanced options', 1
RECONFIGURE;
EXEC sp_configure 'clr strict security', 0;
RECONFIGURE;
Go

select dbo.AddGstTax(10.2);