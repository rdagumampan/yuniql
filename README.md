# yuniql ![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql?style=flat-square&logo=appveyor) [![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/rdagumampan/yuniql?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql/build/tests) [![Gitter](https://img.shields.io/gitter/room/yuniql/yuniql?style=flat-square&logo=gitter&color=orange)](https://gitter.im/yuniql/yuniql) [![Download latest build](https://img.shields.io/badge/Download-win--x64-green?style=flat-square&logo=windows)](https://github.com/rdagumampan/yuniql/releases/download/latest/yuniql-cli-win-x64-latest-full.zip) [![Download latest build](https://img.shields.io/badge/Download-docker--images-green?style=flat-square&logo=docker)](https://hub.docker.com/r/rdagumampan/yuniql)

>*** Disclaimer: **`yuniql`** is not yet officially released. Much of the claims here are still work in progress but nightly build have major features available.

<img align="right" src="assets/yuniql-logo.png" width="150">

**yuniql** (yuu-nee-kel) is a schema versioning and database migration tool for sql server and others. Versions are organized as series of ordinary directories or folders. Scripts are stored transparently as plain old `.sql` files. Yuniql simply automates what you would normally do by hand and executes scripts in an orderly and transactional fashion.

Yuniql promotes and facilitates an end-to-end database DevOps discipline. From schema versioning, to fresh database provisioning and releases via continuous delivery pipeline tasks.

<img align="center" src="https://github.com/rdagumampan/yuniql/raw/master/assets/wiki-evodb-01.png" width="700">

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

## Working with CLI
Manage local db versions and run database migrations from your CLI tool. Perform local migration run or verify with uncommitted runs to test your scripts. Install yuniql CLI with Chocolatey or use alternative ways listed here https://github.com/rdagumampan/yuniql/wiki/Install-yuniql

```console
choco install yuniql --version 0.328.0
```

#### Run migrations for SQL Server

```console
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!"
```

```console
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql-cli
cd c:\temp\yuniql-cli\samples\basic-sqlserver-sample

yuniql run -a
yuniql info
```

#### Run migrations for PostgreSql, MySql and others

```console
docker run -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
SETX YUNIQL_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
```

```console
cd c:\temp\yuniql-cli\samples\basic-postgresql-sample

yuniql run -a --platform postgresql
yuniql info
```

## Working with Azure DevOps Pipelines Tasks
Run your database migration from Azure DevOps Pipelines. The tasks downloads package and cache it for later execution just like how `Use .NET Core` or `Use Node` tasks works. Find Yuniql on [Azure DevOps MarketPlace](https://marketplace.visualstudio.com/items?itemName=rdagumampan.yuniql-azdevops-extensions). Developer guide available here https://github.com/rdagumampan/yuniql/wiki/How-to-run-migration-from-Azure-Devops.

<img align="center" src="https://github.com/rdagumampan/yuniql/raw/master/yuniql-azure-pipelines/images/screenshot-02.png" width="700">

## Working with Docker Container
Run your database migration thru a docker container. This is specially helpful on Linux environments and CI/CD pipelines running on Linux Agents as it facilitates your migration without having to worry any local installations or runtime dependencies. Developer guide available here https://github.com/rdagumampan/yuniql/wiki/How-to-run-migration-from-docker-container.

```console
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql-docker
cd c:\temp\yuniql-docker\samples\basic-sqlserver-sample

docker build -t sqlserver-example .
docker run sqlserver-example -c "<your-connection-string>" -a --platform sqlserver
```

## Working with ASP.NET Core
Run your database migration when your ASP.NET Core host service starts up. This ensures that database is always at latest compatible state before operating the service. Applies to Worker and WebApp projects. Developer guide available here https://github.com/rdagumampan/yuniql/wiki/How-to-run-migration-from-ASP.NET-Core.

```console
dotnet add package Yuniql.AspNetCore
```

```csharp
using Yuniql.AspNetCore;
...
...

//docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
var traceService = new ConsoleTraceService { IsDebugEnabled = true };
app.UseYuniql(traceService, new YuniqlConfiguration
{
	WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
	ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
	AutoCreateDatabase = true,
	Tokens = new List<KeyValuePair<string, string>> {
		new KeyValuePair<string, string>("VwColumnPrefix1","Vw1"),
		new KeyValuePair<string, string>("VwColumnPrefix2","Vw2"),
		new KeyValuePair<string, string>("VwColumnPrefix3","Vw3"),
		new KeyValuePair<string, string>("VwColumnPrefix4","Vw4")
	}
});
```

## Working with Console Application
Run your database migration when Console App starts. Developer guide available here https://github.com/rdagumampan/yuniql/wiki/How-to-run-migration-from-.NET-Core-Console-Application.
 
```console
dotnet add package Yuniql.Core
```

```csharp
using Yuniql.Core;
...
...

static void Main(string[] args)
{
	//docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
	var traceService = new ConsoleTraceService { IsDebugEnabled = true };
	var configuration = new YuniqlConfiguration
	{
		WorkspacePath = Path.Combine(Environment.CurrentDirectory, "_db"),
		ConnectionString = "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!",
		AutoCreateDatabase = true
	};

	var migrationServiceFactory = new MigrationServiceFactory(traceService);
	var migrationService = migrationServiceFactory.Create();
	migrationService.Initialize(configuration.ConnectionString);
	migrationService.Run(
		configuration.WorkspacePath,
		configuration.TargetVersion,
		configuration.AutoCreateDatabase,
		configuration.Tokens,
		configuration.VerifyOnly,
		configuration.Delimiter);
}
```

## Advanced use cases

* [How to bulk import data](https://github.com/rdagumampan/yuniql/wiki/How-to-bulk-import-data-during-migration)
* [How to replace tokens in script files](https://github.com/rdagumampan/yuniql/wiki/How-to-apply-token-replacement)
* [How to version your database](https://github.com/rdagumampan/yuniql/wiki/How-to-baseline-your-database)
* [How yuniql works](https://github.com/rdagumampan/yuniql/wiki/How-yuniql-works)

## Ask for help or contribute

You may submit ideas for improvement or report a bug by [creating an issue](https://github.com/rdagumampan/yuniql/issues/new). <br>
If you have questions, talk to us on [gitter chat](https://gitter.im/yuniql/community). Alternatively, tag [#yuniql](https://twitter.com/) on Twitter.

## Supported databases and platform tests
For running migration from docker container, [see instructions here](https://github.com/rdagumampan/yuniql/wiki/How-to-run-migration-from-a-docker-container)

|Platform|Build Status|Description|
|---|---|---|
|SqlServer|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-14iom?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-14iom/build/tests)|Sql Server 2017, Azure SQL Database|
|PostgreSql|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-w1l3j?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-w1l3j/build/tests)|PostgreSql v9.6, v12.1|
|MySql|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-xk6jt?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-xk6jt/build/tests)|MySql v5.7, v8.0|
|Docker image linux-x64|![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql-ee37o?style=flat-square&logo=appveyor)|`docker pull rdagumampan/yuniql:linux-x64-latest`|
|Docker imiage win-x64|![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql-uakd6?style=flat-square&logo=appveyor)|`docker pull rdagumampan/yuniql:win-x64-latest`|

* Amazon RDS - Aurora ***
* Snowflake Data Warehouse ***
* Azure SQL Data Warehouse ***

*** planned or being evaluated/developer/tested

## License

Copyright (C) 2019 Rodel E. Dagumampan

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

## Credits

Yuniql relies on many open-source projects and we would like to thanks:
- [CommandlineParser](https://github.com/commandlineparser) for CLI commands
- [CsvTextFieldParser](https://github.com/22222/CsvTextFieldParser) for CSV file parsing
- [Npgsql](https://github.com/npgsql/npgsql) for PostgreSql drivers
- [Shouldly](https://github.com/shouldly) for unit tests
- [Moq](https://github.com/moq) for unit test mocks
- Microsoft, Oracle, for everything in dotnetcore seems open source now :)
- All the free devops tools! GitHub, AppVeyor, Docker Shields.io++

## Maintainers

[//]: contributor-faces

<a href="https://github.com/rdagumampan"><img src="https://avatars.githubusercontent.com/u/5895952?v=3" title="rdagumampan" width="80" height="80"></a>

[//]: contributor-faces
