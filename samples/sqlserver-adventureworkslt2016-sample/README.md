# sqlserver-adventureworkslt2016-sample

For more how-to guides, samples and developer guides, walk through our [documentation](https://yuniql.io/docs) and bookmark [https://yuniql.io](https://yuniql.io).

Deploy local sql-server with docker or use your local instance

```console
docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=helloyuniql;User Id=SA;Password=P@ssw0rd!"
```

Run migrations

```console
yuniql run -a
```

Verify migrations

```console
yuniql list

Running yuniql v1.0.1 for windows-x64
Copyright 2019 (C) Rodel E. Dagumampan. Apache License v2.0
Visit https://yuniql.io for documentation & more samples

+---------------+----------------------+------------+---------------+---------------------+
| SchemaVersion | AppliedOnUtc         | Status     | AppliedByUser | AppliedByTool       |
+---------------+----------------------+------------+---------------+---------------------+
| v0.00         | 2020-07-01 21:34:00Z | Successful | sa            | yuniql-cli v1.0.1.0 |
| v0.01         | 2020-07-01 21:34:01Z | Successful | sa            | yuniql-cli v1.0.1.0 |
+---------------+----------------------+------------+---------------+---------------------+

INF   2020-07-01 21:35:47Z   Listed all schema versions applied to database on  workspace.
For platforms not supporting full transactional DDL operations (ex. MySql, CockroachDB, Snowflake), unsuccessful migrations will show the status as Failed and you can look for LastFailedScript and LastScriptError in the schema version tracking table.

```

## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
