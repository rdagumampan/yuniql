# basic-sqlserver-sample

This project is created and to be executed thru `yuniql` CLI tool. 

- To install `yuniql cli`, see https://yuniql.io/docs/install-yuniql
- To format connection string, see https://www.connectionstrings.com.

Let start by cloning this sample repo to your local

```console
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql
cd c:\temp\yuniql\samples\basic-sqlserver-sample
```

Deploy local sqlserver with docker or use your own server and set environment variable

```console
docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!"
```

Run migrations

```console
yuniql run -a
```

Show migrations applied

```console
yuniql list
```

Erase database

```console
yuniql erase -f
```

## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
