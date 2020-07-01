# aspnetcore-sample

This is a sample project for running PostgreSql database migrations in ASP.NET Core app.
For more how-to guides, samples and developer guides, walk through our [documentation](https://yuniql.io/docs) and bookmark [https://yuniql.io](https://yuniql.io).

Deploy local postgresql with docker or use your local instance

```console
docker run -dit -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
```

Build and run

```console
dotnet build
dotnet run
```

## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
