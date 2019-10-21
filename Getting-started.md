# Getting started

## Prepare your environment

### Deploy a sql server linux container
	
```console
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Manila2050!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
docker ps

CONTAINER ID            IMAGE                                        PORTS                 
<dynamic-container-id>  mcr.microsoft.com/mssql/server:2017-latest   0.0.0.0:1400->1433/tcp
```

### Configure your connection string

```
SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=HelloYuniqlDb;User Id=SA;Password=Manila2050!" 
```

## Download yuniql

```powershell
powershell Invoke-WebRequest -Uri https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-nightly.zip -OutFile  "c:\temp\yuniql-nightly.zip"
powershell Expand-Archive "c:\temp\yuniql-nightly.zip" -DestinationPath "c:\temp\yuniql-nightly"
cd c:\temp\yuniql-nightly
```

## Initialize your workspace
```
yuniql-nightly> yuniql init
yuniql-nightly> dir /O:N

INF   2019-10-21T20:41:49.3420320Z   Created script directory c:\temp\yuniql-nightly\_init
INF   2019-10-21T20:41:49.3425548Z   Created script directory c:\temp\yuniql-nightly\_pre
INF   2019-10-21T20:41:49.3433916Z   Created script directory c:\temp\yuniql-nightly\v0.00
INF   2019-10-21T20:41:49.3439002Z   Created script directory c:\temp\yuniql-nightly\_draft
INF   2019-10-21T20:41:49.3443766Z   Created script directory c:\temp\yuniql-nightly\_post
INF   2019-10-21T20:41:49.3462972Z   Created file c:\temp\yuniql-nightly\README.md
INF   2019-10-21T20:41:49.3467579Z   Created file c:\temp\yuniql-nightly\Dockerfile
INF   2019-10-21T20:41:49.3468992Z   Initialized c:\temp\yuniql-nightly

```

## Increment major version
```
yuniql-nightly> yuniql vnext -M
yuniql-nightly> dir /O:N

10/21/2019  22:41    <DIR>          _draft
10/21/2019  22:41    <DIR>          _init
10/21/2019  22:41    <DIR>          _post
10/21/2019  22:41    <DIR>          _pre
10/21/2019  22:41    <DIR>          v0.00
10/21/2019  22:46    <DIR>          v1.00
10/21/2019  22:41                 Dockerfile
10/21/2019  22:41                 README.md
```

## Create your first script inside `v1.00`

```
//setup_tables.sql
CREATE TABLE Visitor (
	VisitorID INT IDENTITY(1000,1),
	FirstName NVARCHAR(255),
	LastName VARCHAR(255),
	Address NVARCHAR(255),
	Email NVARCHAR(255)
);
```

## Run migration
```
yuniql-nightly> yuniql run -a -c "Server=localhost,1400;Database=HelloYuniqlDb;User Id=SA;Password=Manila2050!"
yuniql-nightly> yuniql info -c "Server=localhost,1400;Database=HelloYuniqlDb;User Id=SA;Password=Manila2050!"

Version         Created                         CreatedBy
v0.00           2019-10-21T21:16:48.8130000     sa
v1.00           2019-10-21T21:16:49.4130000     sa
```

## Increment minor version
```
yuniql-nightly> yuniql vnext
yuniql-nightly> dir /O:N

10/21/2019  22:41    <DIR>          v0.00
10/21/2019  22:46    <DIR>          v1.00
10/21/2019  22:46    <DIR>          v1.01
```

## Create your second script inside `v1.01`

```
//initialize_tables.sql
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Jack','Poole','Manila','jack.poole@never-exists.com')
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Diana','Churchill','Makati','diana.churchill@never-exists.com')
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Rebecca','Lyman','Rizal','rebecca.lyman@never-exists.com')
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Sam','Macdonald','Batangas','sam.macdonald@never-exists.com')
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Matt','Paige','Laguna','matt.paige@never-exists.com')
```

## Run migration
```
yuniql-nightly> yuniql run -a -c "Server=localhost,1400;Database=HelloYuniqlDb;User Id=SA;Password=Manila2050!"
yuniql-nightly> yuniql info

|---|---|---|---|---|
|VisitorID	    |FirstName	|LastName	|Address	    |Email|
|---|---|---|---|---|
|1000|Jack	    |Poole	    |Manila	    |jack.poole@never-exists.com|
|1001|Diana	    |Churchill	|Makati	    |diana.churchill@never-exists.com|
|1002|Rebecca	|Lyman	    |Rizal	    |rebecca.lyman@never-exists.com|
|1003|Sam	    |Macdonald	|Batangas	|sam.macdonald@never-exists.com|
|1004|Matt	    |Paige	    |Laguna	    |matt.paige@never-exists.com|
```

## Initialize git repo

```git
yuniql-nightly> git init
yuniql-nightly> git add -A
yuniql-nightly> git commit -m "This is my first yuniql migration"
```

## Create destination git repo and push your changes

```
yuniql-nightly> git remote add origin git@github.com:<your-github-account>/<your-repository-name>.git
yuniql-nightly> git push -u origin master
```

## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).