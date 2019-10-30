# yuniql baseliner

Baseline version automation of your sql server database.

```console
SETX YUNIQLX_BASELINE_CONNECTION_STRING <your-connection-string>
yuniqlx baseline -p "c:\demo\v0.00"
```

```console
yuniqlx baseline -p "c:\demo\v0.00" -c <your-connection-string>
```

Publish as self-contained application (win-x64)
```console
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true
```

Publish as optimized application (linux-x64)
```console
dotnet publish -c release -r linux-x64 /p:publishtrimmed=true
```

#### References

https://docs.microsoft.com/en-us/sql/linux/tutorial-restore-backup-in-sql-server-container?view=sql-server-ver15
https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks2016.bak
https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorksLT2016.bak
https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorksDW2016.bak