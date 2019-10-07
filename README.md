
# yuniql - straight forward sql server database migration

### Motivation
- ridiculously simple! you just need to know sql
- true cross-platform, runs on windows and linux
- zero dependencies, self-contained app, no jvm!
- devops ready, use your own version control and pipelines framework
- open source, MIT license

### Supported commands
```console
yuniql init
yuniql init -p c:\temp\demo | --path c:\temp\demo
yuniql vnext
yuniql vnext -p c:\temp\demo | --path c:\temp\demo
yuniql vnext -M | --major
yuniql vnext -m | --minor
yuniql vnext -f "Table1.sql"
yuniql run
yuniql run -a true | --auto-create-db true
yuniql run -p c:\temp\demo | --path c:\temp\demo
yuniql run -t v1.05 | --target-version v1.05
yuniql run -c "<connectiong-string>"
yuniql -v | --version
yuniql -h | --help
yuniql -d | --debug
```

### Run from windows (self-contained/.exe/win-x64)
```console
yuniql init
yuniql vnext
yuniql vnext -M
yuniql vnext -a
yuniql run -a
```

### Run from windows (dotnet/.netcore 3.0)
```console
dotnet "yuniql.dll" "init" -p "c:\temp\demo"
dotnet "yuniql.dll" "vnext" -p "c:\temp\demo"
dotnet "yuniql.dll" "vnext" -M -p "c:\temp\demo"
dotnet "yuniql.dll" "vnext" -m -p "c:\temp\demo"
dotnet "yuniql.dll" "run" -p "c:\temp\demo" -a
```

### Run from linux
```console
dotnet "yuniql.dll" "run" -p "c:\temp\demo" -a
```

### Publish as self-contained application (.exe)
```console
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true
```

### Deploy SQL Server on Docker container and run a yuniql migration

Connection strings:
- Local instance: Data Source=.;Integrated Security=SSPI;Initial Catalog=YuniqlDemoDB
- Docker container: Server=localhost,1401;Database=YuniqlDemoDB;User Id=SA;Password=P@ssw0rd!;

```console
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1401:1433 -d mcr.microsoft.com/mssql/server:2017-latest

docker build -t rdagumampan/yuniql-tests .
docker run -d -i -t rdagumampan/yuniql-tests
```

### todo
- automated tests
- integrate with `dotnet tool` such as `dotnet tool install -g yunisql`
- package as nuget that app can run during startup
- deploy docker container and run migration
- yuniql --info
- support placeholders ${ENV.VariableName}, ${VariableleName}
- bulk load from csv files

## License
MIT

## Contributing
TBA

