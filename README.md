# yuniql ![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql?style=flat-square&logo=appveyor) [![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/rdagumampan/yuniql?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql/build/tests) [![Gitter](https://img.shields.io/gitter/room/yuniql/yuniql?style=flat-square&logo=gitter&color=orange)](https://gitter.im/yuniql/yuniql) [![Download latest build](https://img.shields.io/badge/Download-win--x64-green?style=flat-square&logo=windows)](https://github.com/rdagumampan/yuniql/releases/download/latest/yuniql-cli-win-x64-latest.zip) [![Download latest build](https://img.shields.io/badge/Download-docker--images-green?style=flat-square&logo=docker)](https://hub.docker.com/r/yuniql/yuniql)

**** HELP DATA DEVELOPERS DISCOVER YUNIQL, PLEASE STAR THIS REPO. ITS FREE. :) THANKS! ****

**yuniql** (yuu-nee-kel). Free and open source schema versioning and database migration engine made natively with .NET Core. Use plain SQL scripts, bulk import CSV, integrate CI/CD pipelines, zero runtime dependencies and works with windows and linux. Supports SqlServer, PostgreSql, MySql, MariaDB, Snowflake, Redshift and Oracle*.

<!--
<img align="center" src="https://yuniql.io/images/evodb-01.png" width="700">
>Inspired by [Evolutionary Database Design](https://www.martinfowler.com/articles/evodb.html) by Martin Fowler and Pramod Sadalage.
-->

<img align="center" src="https://yuniql.io/images/screen-gif-01-gh.gif" width="100%">

## Working with CLI

Manage local db versions and run database migrations from your CLI tool. Perform local migration run or verify with uncommitted runs to test your scripts. [Download latest release here](https://github.com/rdagumampan/yuniql/releases/download/latest/yuniql-cli-win-x64-latest.zip). Install yuniql CLI with Chocolatey or use alternative ways listed here https://yuniql.io/docs/install-yuniql

```console
dotnet tool install -g yuniql.cli
choco install yuniql
docker run --rm yuniql/cli:linux-x64-latest run --platform sqlserver --help
```

```powershell
# powershell
Invoke-WebRequest -Uri https://github.com/rdagumampan/yuniql/releases/download/latest/yuniql-cli-win-x64-latest.zip -OutFile  "c:\temp\yuniql-win-x64-latest.zip"
Expand-Archive "c:\temp\yuniql-win-x64-latest.zip" -DestinationPath "c:\temp\yuniql-cli-latest"
cd c:\temp\yuniql-cli-latest
.\yuniql.exe run --platform sqlserver --help
```

### Run migrations for SQL Server

```console
docker run -d -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!;TrustServerCertificate=True"
SETX YUNIQL_WORKSPACE "c:\temp\yuniql-cli\samples\basic-sqlserver-sample"
```

```console
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql-cli
cd c:\temp\yuniql-cli\samples\basic-sqlserver-sample

yuniql run -a --platform sqlserver
yuniql list --platform sqlserver

Running yuniql v1.0.1 for windows-x64
Copyright 2019 (C) Rodel E. Dagumampan. Apache License v2.0
Visit https://yuniql.io for documentation & more samples

+---------------+----------------------+------------+---------------+----------------------+--------------+
| SchemaVersion | AppliedOnUtc         | Status     | AppliedByUser | AppliedByTool        | Duration     |
+---------------+----------------------+------------+---------------+----------------------+--------------+
| v0.00         | 2021-02-04 06:06:46Z | Successful | sa            | yuniql-cli v1.1.55.0 | 164 ms / 0 s |
+---------------+----------------------+------------+---------------+----------------------+--------------+
```

### Supported databases and platform tests

Amazon Aurora, Azure Synapse and Alibaba Aspara are being evaluated/developed/tested. For running migration from docker container, [see instructions here](https://yuniql.io/docs/migrate-via-docker-container/).

|Platforms|Build Status|Description|Cloud Infrastructure|Documentation|
|---|---|---|---|---|
|sqlserver|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-14iom?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-14iom/build/tests)|Sql Server 2017 and later |Azure, GCP, AWS|[Get started](https://yuniql.io/docs/get-started-sqlserver/)|
|postgresql|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-w1l3j?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-w1l3j/build/tests)|PostgreSql v9.6 and later |Azure, GCP, AWS|[Get started](https://yuniql.io/docs/get-started-postgresql/)|
|mysql|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-xk6jt?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-xk6jt/build/tests)|MySql v5.7 and later |Azure, GCP, AWS|[Get started](https://yuniql.io/docs/get-started-mysql/)|
|mariadb|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-9v8am?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-9v8am/build/tests)|MariaDb v10.2 and later |Azure, GCP, AWS|[Get started](https://yuniql.io/docs/get-started-mysql/)|
|snowflake|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-16r99?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-16r99/build/tests)|Last verified  12.2021 |GCP|[Get started](https://yuniql.io/docs/get-started-snowflake/)|
|redshift|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-0shgd?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-0shgd/build/tests)|Last verified  12.2021|AWS|[Get started](https://yuniql.io/docs/get-started-resdshift/)|
|oracle|[![yuniql-build-status](https://img.shields.io/appveyor/tests/rdagumampan/yuniql-r1lu4?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/rdagumampan/yuniql-r1lu4/build/tests)|Preview build. Oracle 11g and later|Oracle Cloud|[Get started](https://yuniql.io/docs/get-started-oracle/)|
|sap hana|Analysis phase|Expected Q1 2022|Azure||
|synapse|Analysis phase|Expected Q2 2022|Azure||
|aurora|Ideation phase|No release date yet|AWS||
|asparadb|Ideation phase|No release date yet|Alibaba||

> NOTE: Supported cloud platforms are based on limited testing and community feedbacks where users indicated the platform they were attempting to run against. yuniql primarily uses Amazon RDS as cloud provider for non-vendor specific platforms.

|Distributions|Build Status|Description|
|---|---|---|
|Docker image linux-x64|![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql-ee37o?style=flat-square&logo=appveyor)|`docker pull yuniql/yuniql:linux-x64-latest`|
|Docker imiage win-x64|![yuniql-build-status](https://img.shields.io/appveyor/ci/rdagumampan/yuniql-uakd6?style=flat-square&logo=appveyor)|`docker pull yuniql/yuniql:win-x64-latest`|

### Run migrations for PostgreSql, MySql and others

```console
docker run -d -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
SETX YUNIQL_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
SETX YUNIQL_WORKSPACE "c:\temp\yuniql-cli\samples\basic-postgresql-sample"
```

```console
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql-cli
cd c:\temp\yuniql-cli\samples\basic-postgresql-sample

yuniql run -a --platform postgresql
yuniql list --platform postgresql
```

## Working with Azure DevOps Pipelines Tasks
Run your database migration from Azure DevOps Pipelines. The tasks downloads package and cache it for later execution just like how `Use .NET Core` or `Use Node` tasks works. Find Yuniql on [Azure DevOps MarketPlace](https://marketplace.visualstudio.com/items?itemName=rdagumampan.yuniql-azdevops-extensions). Developer guide is available here https://yuniql.io/docs/migrate-via-azure-devops-pipelines.

<img align="center" src="https://rdagumampan.gallerycdn.vsassets.io/extensions/rdagumampan/yuniql-azdevops-extensions/1.187.0/1593377792822/images/yuniql-run.png">

## Working with Docker Container
Run your database migration thru a Docker container. This is specially helpful on Linux environments and CI/CD pipelines running on Linux Agents as it facilitates your migration without having to worry any local installations or runtime dependencies. Developer guide is available here https://yuniql.io/docs/migrate-via-docker-container.

```console
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql-docker
cd c:\temp\yuniql-docker\samples\basic-sqlserver-sample

docker build -t sqlserver-example .
docker run sqlserver-example -c "<your-connection-string>" -a --platform sqlserver
```

## Working with ASP.NET Core
Run your database migration when your ASP.NET Core host service starts up. This ensures that database is always at latest compatible state before operating the service. Applies to Worker and WebApp projects. Developer guide is available here https://yuniql.io/docs/migrate-via-aspnetcore-application.

```console
dotnet add package Yuniql.AspNetCore
```

```csharp
using Yuniql.AspNetCore;
...
...

//1. deploy new sql server on docker or use existing instance
//$ docker run -dit --name yuniql-sqlserver  -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest

//2. create custom trace message sinks, this can be your own logger framework
var traceService = new ConsoleTraceService { IsDebugEnabled = true };

//3. run migrations
app.UseYuniql(traceService, new Yuniql.AspNetCore.Configuration
{
	Platform = SUPPORTED_DATABASES.SQLSERVER,
	Workspace = Path.Combine(Environment.CurrentDirectory, "_db"),
	ConnectionString = "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!;TrustServerCertificate=True",
	IsAutoCreateDatabase = true, IsDebug = true
});
```

## Working with .NET Core Console Application
Run your database migration when Console App starts. Developer guide is available here https://yuniql.io/docs/migrate-via-netcore-console-application.
 
```console
dotnet add package Yuniql.Core
```

```csharp
using Yuniql.Core;
...
...

static void Main(string[] args)
{
	//1. deploy new sql server on docker or use existing instance
	//$ docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest

	//2. create custom trace message sinks, this can be your own logger framework
	var traceService = new ConsoleTraceService { IsDebugEnabled = true };

	//3. configure your migration run
	var configuration = Configuration.Instance;
	configuration.Platform = SUPPORTED_DATABASES.SQLSERVER;
	configuration.Workspace = Path.Combine(Environment.CurrentDirectory, "_db");
	configuration.ConnectionString = "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!;TrustServerCertificate=True";
	configuration.IsAutoCreateDatabase = true;

	//4. run migrations
	var migrationServiceFactory = new MigrationServiceFactory(traceService);
	var migrationService = migrationServiceFactory.Create();
	migrationService.Run();
```

## Advanced use cases

* [How to bulk import CSV master data](https://yuniql.io/docs/bulk-import-csv-master-data/)
* [How to replace tokens in script files](https://yuniql.io/docs/token-replacement/)
* [How to run environment-aware migrations](https://yuniql.io/docs/environment-aware-scripts/)
* [How to baseline your database](https://yuniql.io/docs/baseline-database/)
* [How yuniql works](https://yuniql.io/docs/how-yuniql-works/)

## Contributing & asking for help

Please submit ideas for improvement or report a bug by [creating an issue](https://github.com/rdagumampan/yuniql/issues/new). <br>
Alternatively, tag [#yuniql](https://twitter.com/) on Twitter or drop me a message rdagumampanATgmail.com.

If this is your first time to participate in an open source initiative, you may look at issues labeled as [first timer friendly issues](https://github.com/rdagumampan/yuniql/issues?q=is%3Aissue+is%3Aopen+label%3Afirst-timers-friendly). If you found an interesting case, you can fork this repository, clone to your dev machine, create a local branch, and make Pull Requests (PR) so I can review and merge your changes.

To prepare your dev machine, please visit https://github.com/rdagumampan/yuniql/wiki/Setup-development-environment

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
- [Evolutionary database design](https://www.martinfowler.com/articles/evodb.html) by Martin Fowler and Pramod Sadalage
- Microsoft, Oracle, for everything in dotnetcore seems open source now :)
- All the free devops tools! [GitHub](http://github.com), [AppVeyor](https://www.appveyor.com), [Docker](https://www.docker.com/), [Shields.io](https://shields.io/) ++

## Maintainers

[//]: contributor-faces

<a href="https://github.com/rdagumampan"><img src="https://avatars.githubusercontent.com/u/5895952?v=3" title="rdagumampan" width="80" height="80"></a>

[//]: contributor-faces
