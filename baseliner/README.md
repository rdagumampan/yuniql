# yuniql baseliner

Baseline version automation of your sql server database.

Supported commands

```console
yuniqlx baseline
yuniqlx rebase
```

#### References

https://docs.microsoft.com/en-us/sql/linux/tutorial-restore-backup-in-sql-server-container?view=sql-server-ver15
https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks2016.bak
https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorksLT2016.bak
https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorksDW2016.bak

FILE LIST ONLY
```powershell
docker exec -it baseliner_db_1 /opt/mssql-tools/bin/sqlcmd -S localhost `
   -U SA -P "P@ssw0rd!" `
   -Q "RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/backup/wwi.bak'"

docker 
```

Publish as self-contained application (win-x64)
```console
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true
```
