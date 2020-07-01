# basic-postgresql-sample

This project is created and to be executed thru `yuniql` CLI tool.

- To install `yuniql cli`, see https://yuniql.io/docs/install-yuniql
- To format connection string, see https://www.connectionstrings.com.

Let start by cloning this sample repo to your local

```console
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql
cd c:\temp\yuniql\samples\basic-postgresql-sample
```

Deploy local postgresql with docker or use your own server and set environment variable

```console
docker run -dit --name yuniql-postgresql -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=helloyuniql -p 5432:5432 postgres
SETX YUNIQL_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=helloyuniql"
```

Run migrations

```console
yuniql run -a --platform postgresql
```

Show migrations applied

```console
yuniql list --platform postgresql
```

Erase database

```console
yuniql erase -f --platform postgresql
```

## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
