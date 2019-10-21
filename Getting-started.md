## Getting started

#### 1. Prepare your environment

##### 1.1 Deploy a sql server linux container

```bash
docker run 
	-e "ACCEPT_EULA=Y" 
	-e "MSSQL_SA_PASSWORD=Manila2050!" 
	-p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
docker ps

CONTAINER ID            IMAGE                                        PORTS                 
<dynamic-container-id>  mcr.microsoft.com/mssql/server:2017-latest   0.0.0.0:1400->1433/tcp
```

##### 1.2 Configure your connection string

```bash
SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=HelloYuniqlDb;User Id=SA;Password=Manila2050!" 
```

#### 2. Download yuniql

```powershell
powershell Invoke-WebRequest 
	-Uri https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-nightly.zip 
	-OutFile  "c:\temp\yuniql-nightly.zip"
powershell Expand-Archive "c:\temp\yuniql-nightly.zip" -DestinationPath "c:\temp\yuniql-nightly"
cd c:\temp\yuniql-nightly
```

#### 3. Initialize your workspace

```bash
yuniql-nightly> yuniql init
yuniql-nightly> dir /O:N
```

#### 4. Increment major version

```bash
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

#### 5. Create your first script inside `v1.00`

```sql
//setup_tables.sql
CREATE TABLE Visitor (
	VisitorID INT IDENTITY(1000,1),
	FirstName NVARCHAR(255),
	LastName VARCHAR(255),
	Address NVARCHAR(255),
	Email NVARCHAR(255)
);
```

#### 6. Run migration

```bash
yuniql-nightly> yuniql run -a
yuniql-nightly> yuniql info

Version         Created                         CreatedBy
v0.00           2019-10-21T21:16:48.8130000     sa
v1.00           2019-10-21T21:16:49.4130000     sa
```

#### 7. Increment minor version

```bash
yuniql-nightly> yuniql vnext
yuniql-nightly> dir /O:N

10/21/2019  22:41    <DIR>          v0.00
10/21/2019  22:46    <DIR>          v1.00
10/21/2019  22:46    <DIR>          v1.01
```

#### 8. Create your second script inside `v1.01`

```sql
//initialize_tables.sql
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Jack','Poole','Manila','jack.poole@never-exists.com')
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Diana','Churchill','Makati','diana.churchill@never-exists.com')
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Rebecca','Lyman','Rizal','rebecca.lyman@never-exists.com')
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Sam','Macdonald','Batangas','sam.macdonald@never-exists.com')
INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Matt','Paige','Laguna','matt.paige@never-exists.com')
```

#### 9. Run migration again

```bash
yuniql-nightly> yuniql run
yuniql-nightly> yuniql info
```

Verify that records has been inserted as part version `v1.01`
```sql
//SELECT * FROM [dbo].[Visitor]
VisitorID   FirstName   LastName    Address  Email
----------- ----------- ----------- ------------------------------------------
1000        Jack        Poole       Manila   jack.poole@never-exists.com
1001        Diana       Churchill   Makati   diana.churchill@never-exists.com
1002        Rebecca     Lyman       Rizal    rebecca.lyman@never-exists.com
1003        Sam         Macdonald   Batangas sam.macdonald@never-exists.com
1004        Matt        Paige       Laguna   matt.paige@never-exists.com
```
#### 10. Initialize git repo

```git
yuniql-nightly> git init
yuniql-nightly> git add -A
yuniql-nightly> git commit -m "This is my first yuniql migration"
```

#### 11. Create destination git repo and push your changes

```bash
yuniql-nightly> git remote add origin git@github.com:<your-github-account>/<your-repository-name>.git
yuniql-nightly> git push -u origin master
```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
