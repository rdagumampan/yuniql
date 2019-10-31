# yuniql ![yuniql-build-status](https://ci.appveyor.com/api/projects/status/e6hqrhqa6d1lnma0?svg=true) [![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/rdagumampan/yuniql)](https://ci.appveyor.com/project/rdagumampan/yuniql/build/tests) [![Gitter](https://img.shields.io/gitter/room/yuniql/yuniql)](https://gitter.im/yuniql/yuniql) [![Stack Overflow](https://img.shields.io/badge/stack%20overflow-yuniql-green.svg)](http://stackoverflow.com/questions/tagged/yuniql) [![Download latest build](https://ci.appveyor.com/api/projects/status/32r7s2skrgm9ubva?svg=true&passingText=Download%20win-x64)](https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-nightly.zip) [![Download latest build](https://ci.appveyor.com/api/projects/status/32r7s2skrgm9ubva?svg=true&passingText=Download%20linux-x64)](https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-nightly-linux-x64.zip)


>*** Disclaimer: **`yuniql`** is not yet officially released. Much of the claims here are still work in progress but nightly build have major features available.

<img align="right" src="assets/yuniql-logo.png">

**yuniql** is a schema versioning and database migration tool for sql server. All scripts are organized transparently as `.sql` files and versions are arranged as series of ordinary directories. yuniql automates what you would normally do by hand and executes all scripts in an orderly and transactional fashion. yuniql also seamlessly integrates with your Continuous Delivery (CD) pipelines for truely db/devops development experience.

**yuniql** is released as self-contained .NET Core 3.0 application (no need for JVM or .NET CLR) for Windows. Container images are also available for seamless Continuous Delivery of database changes in Linux.

#### Getting started

This 10-step tutorial shows you how to deploy your first sql-based migration into an sql server. For simplicity, we assume you have a Docker service running, a `C:\temp` directory, and `git` client. Estimated completion time: 10 mins.

##### Prepare your environment

1. Deploy a sql server linux container

	```bash
	docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Manila2050!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
	```
	```
	docker ps

	CONTAINER ID            IMAGE                                        PORTS                 
	<dynamic-container-id>  mcr.microsoft.com/mssql/server:2017-latest   0.0.0.0:1400->1433/tcp
	```

2. Configure your connection string

	```bash
	SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=HelloYuniqlDb;User Id=SA;Password=Manila2050!"
	```

##### Run your first migration

1. Download `yuniql`. You may also download manually [here](https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-nightly.zip) and extract to desired directory.

	```powershell
	powershell Invoke-WebRequest -Uri https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-nightly.zip -OutFile  "c:\temp\yuniql-nightly.zip"
	powershell Expand-Archive "c:\temp\yuniql-nightly.zip" -DestinationPath "c:\temp\yuniql-nightly"
	cd c:\temp\yuniql-nightly
	```
	>`Expand-Archive` requires at least powershell v5.0+ running on your machine
2. Initialize your workspace

	```bash
	yuniql-nightly> yuniql init
	yuniql-nightly> dir /O:N
	```

3. Increment major version
The `vnext -M` creates a new major version with format `v{major}.{minor}`. You can of course create this manually inthe directory!

	```bash
	yuniql-nightly> yuniql vnext -M
	yuniql-nightly> dir /O:N

	10/21/2019  22:41    <DIR>          _draft
	10/21/2019  22:41    <DIR>          _init
	10/21/2019  22:41    <DIR>          _post
	10/21/2019  22:41    <DIR>          _pre
	10/21/2019  22:41    <DIR>          v0.00
	10/21/2019  22:46    <DIR>          v1.00
	10/21/2019  22:41                   Dockerfile
	10/21/2019  22:41                   README.md
	10/21/2019  22:41                   .gitignore
	```

4. Create your first script file `setup-tables.sql` inside `v1.00` directory

	```sql
	--setup-tables.sql
	CREATE TABLE Visitor (
		VisitorID INT IDENTITY(1000,1),
		FirstName NVARCHAR(255),
		LastName VARCHAR(255),
		Address NVARCHAR(255),
		Email NVARCHAR(255)
	);
	```

5. Run migration

	```bash
	yuniql-nightly> yuniql run -a
	yuniql-nightly> yuniql info

	Version         Created                         CreatedBy
	v0.00           2019-10-21T21:16:48.8130000     sa
	v1.00           2019-10-21T21:16:49.4130000     sa
	```

6. Increment minor version

	```bash
	yuniql-nightly> yuniql vnext
	yuniql-nightly> dir /O:N

	10/21/2019  22:41    <DIR>          v0.00
	10/21/2019  22:46    <DIR>          v1.00
	10/21/2019  22:46    <DIR>          v1.01
	```

7. Create your second script file `initialize-tables.sql` inside `v1.01` directory

	```sql
	--initialize-tables.sql
	INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Jack','Poole','Manila','jack.poole@never-exists.com')
	INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Diana','Churchill','Makati','diana.churchill@never-exists.com')
	INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Rebecca','Lyman','Rizal','rebecca.lyman@never-exists.com')
	INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Sam','Macdonald','Batangas','sam.macdonald@never-exists.com')
	INSERT INTO [dbo].[Visitor]([FirstName],[LastName],[Address],[Email])VALUES('Matt','Paige','Laguna','matt.paige@never-exists.com')
	```

8. Run migration again

	```bash
	yuniql-nightly> yuniql run
	yuniql-nightly> yuniql info
	```

	Verify that records has been inserted as part version `v1.01` using SSMS or preferred SQL client.
	
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
9. Initialize `git` repo

	```git
	yuniql-nightly> git init
	yuniql-nightly> git add -A
	yuniql-nightly> git commit -m "This is my first yuniql migration"
	```

10. Create destination `git` repo and `push` your changes.
You may use any other git provider and replace the `.git` folder.

	<img src="assets/yuniql-test-repo.png" width="450" height="200">

	```bash
	yuniql-nightly> git remote add origin https://github.com/{your-github-account}/{your-github-database-repo}.git
	yuniql-nightly> git push -u origin master
	```
	>NOTE: For simplicity I use HTTPS mode in setting git repo. If you use SSH, you need to download and configure your keys.

#### Digging deeper

* [Setting up new database](https://github.com/rdagumampan/yuniql/wiki/How-to-baseline-your-database)
* [How yuniql works](https://github.com/rdagumampan/yuniql/wiki/How-yuniql-works)
* [Replace tokens in script files](https://github.com/rdagumampan/yuniql/wiki/How-to-use-yuniql-token-replacement)
* [Bulk import data during migration](https://github.com/rdagumampan/yuniql/wiki/How-to-bulk-load-data-during-migration)
* [Migrate via docker container](https://github.com/rdagumampan/yuniql/wiki/Run-migration-on-Docker-container)
* [Best practices](https://github.com/rdagumampan/yuniql/wiki/Best-practices)

#### `yuniql` CLI commands

```bash
yuniql init
yuniql init -p c:\temp\demo | --path c:\temp\demo
yuniql vnext
yuniql vnext -p c:\temp\demo | --path c:\temp\demo
yuniql vnext -M | --major
yuniql vnext -m | --minor
yuniql vnext -f "Table1.sql"
yuniql run
yuniql run -u | --uncomitted ***
yuniql run -a true | --auto-create-db true
yuniql run -p c:\temp\demo | --path c:\temp\demo
yuniql run -t v1.05 | --target-version v1.05
yuniql run -c "<connectiong-string>"
yuniql run -k "Token1=TokenValue1,Token2=TokenValue2,Token3=TokenValue3"
yuniql -v | --version
yuniql -h | --help
yuniql -d | --debug
```

*** planned or being evaluated/developer/tested

#### `yuniqlx` CLI commands

```bash
yuniqlx baseline -c "<connectiong-string>"
yuniqlx rebase -c "<connectiong-string>" ***
yuniqlx erase -c "<connectiong-string>" ***
```

*** planned or being evaluated/developer/tested

#### Build Status

|Platform|Docker Build Status|
|---|---|
|![linux-x64](assets/linux_med.png) **windows x64**|![yuniql-build-status](https://ci.appveyor.com/api/projects/status/jn7f7r64rx6a2ujk?svg=true)|
|![win-x64](assets/win_med.png) **linux x64**|![yuniql-build-status](https://ci.appveyor.com/api/projects/status/cje10k5mqx4v4emd?svg=true)|

#### How to contribute or ask help
- File a bug or feature as an [issue](https://github.com/rdagumampan/yuniql/issues/new)
- Talk to us on [gitter chat](https://gitter.im/yuniql/community)
- Comment on existing issues and suggest how they should be fixed/implemented
- Fix a bug or implement feature by subimitting a PR
- Write more tests to increase our coverage
- Tag [#yuniql](https://twitter.com/) on Twitter

#### License
TBA
