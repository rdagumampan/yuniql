# basic-sqlserver-sample

This project is created and to be executed thru `yuniql` CLI tool. 

- To install `yuniql cli`, see https://yuniql.io/docs/install-yuniql
- To format connection string, see https://www.connectionstrings.com.

Let start by cloning this sample repo to your local

```console
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql
cd c:\temp\yuniql\samples\basic-oracle-sample
```

Deploy local sqlserver with docker or use your own server and set environment variable

```console
docker run -d -it --name oracle12cdev  -p 1521:1521  store/oracle/database-enterprise:12.2.0.1-slim
SETX YUNIQL_CONNECTION_STRING "Data Source=localhost;User Id=sys;Password=Oradoc_db1;DBA Privilege=SYSDBA;"
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
