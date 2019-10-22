### How to bulk load data during migration

Master data and lookup tables almost comes natural as part of every database provisioning process. With this, you may prepare them in CSV files and yuniql will inspect them and bulk load into tables bearing same name as the CSV file. The following example demonstrates how to do this.

1. Initialize local version

	```bash
	yuniql init
	yuniql vnext
	```

2. Create script file `setup_tables.sql` on `v0.01`

	```sql
	CREATE TABLE Visitor (
		VisitorID INT IDENTITY(1000,1),
		FirstName NVARCHAR(255),
		LastName VARCHAR(255),
		Address NVARCHAR(255),
		Email NVARCHAR(255)
	);
	```

3. Create a `Visitor.csv` on version `v0.01`

	```csv
	"VisitorID","FirstName","LastName","Address","Email"
	"1000","Jack","Poole","Manila","jack.poole@never-exists.com"
	"1001","Diana","Churchill","Makati","diana.churchill@never-exists.com"
	"1002","Rebecca","Lyman","Rizal","rebecca.lyman@never-exists.com"
	"1003","Sam","Macdonald","Batangas","sam.macdonald@never-exists.com"
	"1004","Matt","Paige","Laguna","matt.paige@never-exists.com"
	```

	>NOTE: The file name of the CSV file must match the destination table else an exception is thrown.

4. Run migration

	```bash
	yuniql run -a -c "<your-connection-string>"
	
	INF   2019-10-22T18:36:08.7621330Z   Executed script file C:\temp\yuniql-nightly\v0.01\setup-tables.sql.
	INF   2019-10-22T18:36:08.7638901Z   Found the 1 csv files on C:\temp\yuniql-nightly\v0.01
	INF   2019-10-22T18:36:08.7649367Z   Visitor.csv
	INF   2019-10-22T18:36:09.0854032Z   Imported csv file C:\temp\yuniql-nightly\v0.01\Visitor.csv.
	```

5. Verify if all is good

	```sql
	SELECT * FROM [dbo].[Visitor]
	```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
