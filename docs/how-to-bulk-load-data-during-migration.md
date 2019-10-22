### How to bulk load data during migration

Master data and lookup table data almost comes natural as part of every database provisioning process. With this, you may prepare them in CSV files and yuniql will inspect them and bulk load into tables bearing same name as the CSV file. The following example demonstrates how to do this

1. Initialize local version

	```bash
	yuniql init
	yuniql vnext
	```

2. Create script file on `v0.01`

	```sql
	--setup_tables.sql
	CREATE TABLE Visitor (
		VisitorID INT IDENTITY(1000,1),
		FirstName NVARCHAR(255),
		LastName VARCHAR(255),
		Address NVARCHAR(255),
		Email NVARCHAR(255)
	);
	```

3. Create a `visitor-data.csv` on version `v0.01`

	```csv
	"VisitorID","FirstName","LastName","Address","Email"
	"1000","Jack","Poole","Manila","jack.poole@never-exists.com"
	"1001","Diana","Churchill","Makati","diana.churchill@never-exists.com"
	"1002","Rebecca","Lyman","Rizal","rebecca.lyman@never-exists.com"
	"1003","Sam","Macdonald","Batangas","sam.macdonald@never-exists.com"
	"1004","Matt","Paige","Laguna","matt.paige@never-exists.com"
	```

4. Run migration

	```bash
	yuniql run -a -c "<your-connection-string>"
	yuniql info
	```

5. Verify if all is good

	```sql
	SELECT * FROM [dbo].[Visitor]
	```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
