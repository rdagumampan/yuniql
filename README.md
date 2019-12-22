# yuniql ![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql?style=flat-square&logo=appveyor) [![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/rdagumampan/yuniql?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql/build/tests) [![Gitter](https://img.shields.io/gitter/room/yuniql/yuniql?style=flat-square&logo=gitter&color=orange)](https://gitter.im/yuniql/yuniql) [![Download latest build](https://img.shields.io/badge/Download-win--x64-green?style=flat-square&logo=windows)](https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-nightly.zip) [![Download latest build](https://img.shields.io/badge/Download-docker--images-green?style=flat-square&logo=docker)](https://hub.docker.com/r/rdagumampan/yuniql)

>*** Disclaimer: **`yuniql`** is not yet officially released. Much of the claims here are still work in progress but nightly build have major features available.

<img align="right" src="assets/yuniql-logo.png" width="200">

**yuniql** (yuu-nee-kel) is a schema versioning and database migration tool for sql server and others. Versions are organized as series of ordinary directories and scripts are stored transparently as `.sql` files. Yuniql simply automates what you would normally do by hand and executes scripts in an orderly and transactional fashion.

## Why yuniql?
- **It's raw SQL.** Yuniql follows database-first approach to version your database. Versions are normal directories or folders. Scripts are series of plain old .sql files. No special tool or language required.
- **It's .NET Core Native.** Released as a self-contained .NET Core 3.0 application. Yuniql doesn't require any dependencies or CLR installed on the developer machine or CI/CD server. For windows, `yuniql.exe` is ready-for-use on day 1.
- **Bulk Import CSV.** Load up your master data and lookup tables from CSV files. A powerful feature when provisioning fresh developer databases or when taking large block of master data as part of a new version.
- **DevOps Friendly.** Azure Pipeline Tasks available in the Market Place. `Use Yuniql` task acquires a specific version of the Yuniql. `Run Yuniql` task runs database migrations with Yuniql CLI using version acquired earlier.
- **Cloud Ready.** Platform tested for Azure SQL Database. Plugins for Amazon RDS and Google Cloud SQL are lined up for development. ***
- **Docker Support.** Each project is prepared for containerized execution using Yuniql base images. A dockerized database project is cheap way to run migration on any CI/CD platform.
- **Cross-platform.** Works with Windows and major Linux distros.
- **Open Source.** Released under Apache License version 2.0. Absolutely free for personal or commercial use.

*** planned or being evaluated/developer/tested

## To start using **`yuniql`** on Sql Server

1. Prepare the connection string to your target sqlserver instance
	- Using an sql account<br>
	`Server=<server-instance>,[<port-number>];Database=VisitorDB;User Id=<sql-user-name>;Password=<sql-user-password>`	
	- Using trusted connection<br>
	`Server=<server-instance>,[<port-number>];Database=VisitorDB;Trusted_Connection=True;`<br><br>

	```bash
	SETX YUNIQL_CONNECTION_STRING "Server=.\;Database=VisitorDB;Trusted_Connection=True;"
	```
2. Clone sample project
	```bash
	git clone https://github.com/rdagumampan/yuniql c:\temp\yuniql
	cd c:\temp\yuniql\sqlserver-samples\visitph-db
	```

3. Download latest `yuniql` build<br>

	```bash
	powershell Invoke-WebRequest -Uri https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-latest-win-x64.zip -OutFile  "c:\temp\yuniql\yuniql-latest-win-x64.zip"
	powershell Expand-Archive "c:\temp\yuniql\yuniql-latest-win-x64.zip" -DestinationPath "c:\temp\yuniql\sqlserver-samples\visitph-db"
	cd c:\temp\yuniql-nightly
	```
	>`Expand-Archive` requires at least powershell v5.0+ running on your machine. You may also [download manually here](https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-latest-win-x64.zip) and extract to desired directory.

4. Run migration<br>
The following commands `yuniql` to discover the project directory, creates the target database if it doesn't exist and runs all migration steps in the order they are listed. These includes `.sql` files, directories, subdirectories, and csv files. Tokens are also replaced via `-k` parameters.
	```bash
	yuniql run -a -k "VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4"
	yuniql info

	Version         Created                         CreatedBy
	v0.00           2019-11-03T16:29:36.0130000     DESKTOP-ULR8GDO\rdagumampan
	v1.00           2019-11-03T16:29:36.0600000     DESKTOP-ULR8GDO\rdagumampan
	v1.01           2019-11-03T16:29:36.1130000     DESKTOP-ULR8GDO\rdagumampan
	```

5. Verify results<br>
Query tables with SSMS or your preferred SQL client
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

	<br>
	<img align="center" src="assets/visitordb-screensot-ssms.png" width="700">

## To start working with **`yuniql`** CLI
See how it works here https://github.com/rdagumampan/yuniql/wiki/How-yuniql-works

```bash
yuniql init
yuniql init -p c:\temp\demo | --path c:\temp\demo
yuniql vnext
yuniql vnext -p c:\temp\demo | --path c:\temp\demo
yuniql vnext -M | --major
yuniql vnext -m | --minor
yuniql vnext -f "your-script-file.sql"
yuniql verify
yuniql run
yuniql run -a true | --auto-create-db true
yuniql run -p c:\temp\demo | --path c:\temp\demo
yuniql run -t v1.05 | --target-version v1.05
yuniql run -c "<connectiong-string>"
yuniql run -k "Token1=TokenValue1,Token2=TokenValue2,Token3=TokenValue3"
yuniql erase
yuniql -v | --version
yuniql -h | --help
yuniql -d | --debug
```

## To dig deeper for advanced use cases

* [How yuniql works](https://github.com/rdagumampan/yuniql/wiki/How-yuniql-works)
* [How to version your database](https://github.com/rdagumampan/yuniql/wiki/How-to-baseline-your-database)
* [How to replace tokens in script files](https://github.com/rdagumampan/yuniql/wiki/How-to-apply-token-replacement)
* [How to bulk import data](https://github.com/rdagumampan/yuniql/wiki/How-to-bulk-import-data-during-migration)
* [How to migrate from docker container](https://github.com/rdagumampan/yuniql/wiki/How-to-run-migration-from-a-docker-container)
* [Known issues](https://github.com/rdagumampan/yuniql/wiki/Known-issues)
* [Best practices](https://github.com/rdagumampan/yuniql/wiki/Best-practices)

## Supported databases

* SQL Server
* Azure SQL Database
* PostgreSQL
* MySQL
* Amazon RDS - Aurora ***

*** planned or being evaluated/developer/tested

## To ask for help or contribute

You may submit ideas for improvement or report a bug by [creating an issue](https://github.com/rdagumampan/yuniql/issues/new). <br>
If you have questions, talk to us on [gitter chat](https://gitter.im/yuniql/community). Alternatively, tag [#yuniql](https://twitter.com/) on Twitter.

## To track platform tests and docker builds
For running migration from docker container, [see instructions here](https://github.com/rdagumampan/yuniql/wiki/How-to-run-migration-from-a-docker-container)

|Platform|Build Status|Description|
|---|---|---|
|SqlServer|![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql-14iom?style=flat-square&logo=appveyor)|Sql Server 2017, Azure SQL Database|
|PostgreSql|![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql-w1l3j?style=flat-square&logo=appveyor)|PostgreSql v9.6|
|MySql|![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql-xk6jt?style=flat-square&logo=appveyor)|MySql v5.7|
|Docker image linux-x64|![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql-ee37o?style=flat-square&logo=appveyor)|`docker pull rdagumampan/yuniql:linux-x64-latest`|
|Docker imiage win-x64|![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql-uakd6?style=flat-square&logo=appveyor)|`docker pull rdagumampan/yuniql:win-x64-latest`|

## License

Copyright (C) 2019 Rodel E. Dagumampan

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

## Maintainers

[//]: contributor-faces

<a href="https://github.com/rdagumampan"><img src="https://avatars.githubusercontent.com/u/5895952?v=3" title="rdagumampan" width="80" height="80"></a>

[//]: contributor-faces
