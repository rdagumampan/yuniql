### How to run these tests

1. Deploy a sql server linux container
	
	```console
	docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
	```

2. Configure your connection string

	```bash
	SETX YUNIQL_CONNECTION_STRING "Server=localhost,1400;Database=HelloYuniqlDb;User Id=SA;Password=P@ssw0rd!"
	```

3. Run the tests
	
	```console
	cd tests
	dotnet build
	dotnet test
	```
#### Found bugs?
Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).