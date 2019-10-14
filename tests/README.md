# How to run these tests

1. Deploy a sql server linux container
	```console
	docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1401:1433 -d mcr.microsoft.com/mssql/server:2017-latest
	```

2. Change the connection string on `TestHelper.cs` if needed

	```csharp
    public static string GetConnectionString(string databaseName)
    {
        //use this when running against local instance of sql server with integrated security
        //return $"Data Source=.;Integrated Security=SSPI;Initial Catalog={databaseName}";

        //use this when running against sql server container with published port 1401
        return $"Server=localhost,1401;Database={databaseName};User Id=SA;Password=P@ssw0rd!";
    }
	```

3. Run the tests
	```console
	dotnet build
	dotnet test
	```

# References

Publish as self-contained application (win-x64)
```console
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true
```

Publish as self-contained application (linux-x64)
```console
dotnet publish -c release -r linux-x64 /p:publishsinglefile=true /p:publishtrimmed=true
```
