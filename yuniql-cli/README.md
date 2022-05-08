# Yuniql.CLI

## Running from your local machine

### Publish the CLI app

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true
```

### Debugging with SqlServer

Deploy a sqlserver container

```console
docker run -dit --name yuniql-sqlserver  -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
```

Run from CLI

```console
cd C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish

SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!;TrustServerCertificate=True"
SETX YUNIQL_WORKSPACE "C:\play\yuniql\samples\basic-sqlserver-sample"
SETX YUNIQL_PLATFORM "sqlserver" 

yuniql run -a --debug
yuniql list --debug
yuniql destroy --force --debug
```

```console
cd C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish

yuniql run -a -p C:\play\yuniql\samples\basic-sqlserver-sample -c "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!" --platform sqlserver --debug
yuniql list -p C:\play\yuniql\samples\basic-sqlserver-sample -c "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!" --platform sqlserver --debug
yuniql destroy --force -c "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!" --platform sqlserver --debug
```


Run from Visual Studio (Debug Mode)

- Set Yuniql.CLI as default startup project
- Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters

Useful VSCode Extensions

- MySql / Jun Han
- SQLTools / Matheus Tixera


### Debugging with PostgreSql

Deploy postgresql container

```console
docker run -dit --name yuniql-postgresql -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=helloyuniql -p 5432:5432 postgres
```

Run from CLI

```console
yuniql run -a -p C:\play\yuniql\samples\basic-postgresql-sample -c "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=helloyuniql" --platform postgresql --debug
yuniql list -c "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=helloyuniql" --platform postgresql --debug
```

Run from Visual Studio (Debug Mode)

- Set Yuniql.CLI as default startup project
- Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters

Install VSCode Extensions

- MySql / Jun Han
- SQLTools / Matheus Tixera

### Debugging with MySql & MariaDb

Deploy mysql container

```console
docker run -dit --name yuniql-mysql -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password
```

Run from CLI

- Set Yuniql.CLI as default startup project
- Open Yuniql.CLI project properties -> Debug -> Add these start-up parameters

```console
yuniql run -a -p C:\play\yuniql\samples\basic-mysql-sample -c "Server=localhost;Port=3306;Database=helloyuniql;Uid=root;Pwd=P@ssw0rd!;" --platform mysql --debug
yuniql list -c "Server=localhost;Port=3306;Database=helloyuniql;Uid=root;Pwd=P@ssw0rd!;" --platform mysql --debug
```

Install VSCode Extensions

- MySql / Jun Han
- SQLTools / Matheus Tixera

### Debugging with Snowflake

Record your Snowflake account information
host=<your-snowflake-host>com;account=<your-snowflake-account>;user=<your-snowflake-user>;password<your-snowflake-pwd>;db=<your-snowflake-db>;schema=PUBLIC

Run from CLI

```console
yuniql run -p C:\play\yuniql\samples\basic-snowflake-sample -c "<your-snowflake-connection-string>" --platform snowflake --debug
yuniql list -c "<your-snowflake-connection-string>" --platform snowflake --debug
```

### Debugging with Redshift

Record your Redshift account information
Server=<your-redshift-instance>.redshift.amazonaws.com;Port=5439;Database=<your-redshift-db>;User Id=<your-redshift-user>;Password=<your-redshift-pwd>;

Run from CLI

```console
yuniql run -p C:\play\yuniql\samples\basic-redshift-sample -c "<your-redshift-connection-string>" --platform snowflake --debug
yuniql list -c "<your-redshift-connection-string>" --platform snowflake --debug
```
### Debugging with Oracle

Record your Redshift account information
Data Source=myOracle;User Id=myUsername;Password=myPassword;DBA Privilege=SYSDBA;

Run from CLI

```console
yuniql run -p C:\play\yuniql\samples\basic-oracle-sample -c "<your-oracle-connection-string>" --platform oracle --debug
yuniql list -c "<your-oracle-connection-string>" --platform oracle --debug
```
