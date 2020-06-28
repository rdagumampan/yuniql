# Yuniql.CLI

## Running from your local machine

### Working with SqlServer

Deploy local database container

```console
docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
```

Set environment variable

```console
SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!"
```

#### Run from CLI

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

cd C:\play\yuniql\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish
yuniql run -a -p C:\play\yuniql\samples\basic-sqlserver-sample --platform sqlserver --debug
```

#### Run from Visual Studio Debug

Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters


```console
run -a -p C:\play\yuniql\samples\basic-sqlserver-sample --platform sqlserver --debug
```


#### Install VSCode Extensions

- MySql / Jun Han
- SQLTools / Matheus Tixera

Browse data from your VS Code

### Working with PostgreSql

Deploy local database container

```console
docker run -dit --name yuniql-postgresql -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=helloyuniql -p 5432:5432 postgres
```

Set environment variable

```console
SETX YUNIQL_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
```

Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters

```console
yuniql run -a -p C:\play\yuniql\samples\basic-postgresql-sample --platform postgresql --debug
```

Install VSCode Extensions

- MySql / Jun Han
- SQLTools / Matheus Tixera

Browse data from your VS Code

### Working with MySql

Deploy local database container

```console
docker run -dit --name mysql -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password
```

Set environment variable

```console
SETX YUNIQL_CONNECTION_STRING "Server=localhost;Port=3306;Database=helloyuniql;Uid=root;Pwd=P@ssw0rd!;"
```

Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters

```console
yuniql run -a -p C:\play\yuniql\samples\basic-mysql-sample --platform mysql --debug
```

Install VSCode Extensions

- MySql / Jun Han
- SQLTools / Matheus Tixera

Browse data from your VS Code