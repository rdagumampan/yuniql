# worker-sample

This is a sample project for running database migrations in .NET Core worker app.
For more how-to guides, samples and developer guides, walk through our [documentation](https://yuniql.io/docs) and bookmark [https://yuniql.io](https://yuniql.io).

Deploy local sql-server with docker or use your local instance

```console
docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
```

Build and run

```console
dotnet build
dotnet run
```

## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
