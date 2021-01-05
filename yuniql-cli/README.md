# Yuniql.CLI

## Running from your local machine

### Debugging with SqlServer

Deploy a sqlserver container

```console
docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
```

#### Run from CLI

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

cd C:\play\yuniql\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish
yuniql run -a -p C:\play\yuniql\samples\basic-sqlserver-sample -c  -c "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!" --platform sqlserver --debug
yuniql list -p C:\play\yuniql\samples\basic-sqlserver-sample -c  -c "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!" --platform sqlserver --debug
```

#### Run from Visual Studio (Debug Mode)

Set Yuniql.CLI as default startup project
Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters

```console
run -a -p "C:\play\yuniql\samples\basic-sqlserver-sample" -c "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!" --platform sqlserver --debug
```

#### Useful VSCode Extensions

- MySql / Jun Han
- SQLTools / Matheus Tixera

Browse data from your VS Code

### Debugging with PostgreSql

Deploy postgresql container

```console
docker run -dit --name yuniql-postgresql -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=helloyuniql -p 5432:5432 postgres
```

Set Yuniql.CLI as default startup project
Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters

```console
yuniql run -a -p C:\play\yuniql\samples\basic-postgresql-sample -c "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb" --platform postgresql --debug
```

Install VSCode Extensions

- MySql / Jun Han
- SQLTools / Matheus Tixera

Browse data from your VS Code

### Debugging with MySql & MariaDb

Deploy mysql container

```console
docker run -dit --name mysql -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password
```

Set Yuniql.CLI as default startup project
Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters

```console
run -a -p C:\play\yuniql\samples\basic-mysql-sample -c "Server=localhost;Port=3306;Database=helloyuniql;Uid=root;Pwd=P@ssw0rd!;" --platform mysql --debug
```

Install VSCode Extensions

- MySql / Jun Han
- SQLTools / Matheus Tixera

Browse data from your VS Code

### Debugging with Snowflake

Record your Snowflake account information
host=<host>com;account=<account>;user=<user-id>;password<password>;db=HELLO_YUNIQL;schema=PUBLIC


Set Yuniql.CLI as default startup project
Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters

```console
run -p C:\play\yuniql\samples\basic-snowflake-sample -c "" --platform snowflake --debug
```