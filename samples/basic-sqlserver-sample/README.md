# basic-sqlserver-sample

This project is created and to be executed thru `yuniql` CLI tool. If you need help formatting connection string, please visit https://www.connectionstrings.com.

Deploy local postgresql with docker or use your own server

```console
docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!"
```

Run migrations

```console
yuniql run -a
```

Show migrations

```console
yuniql list
```

Erase database

```console
yuniql erase -f
```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
