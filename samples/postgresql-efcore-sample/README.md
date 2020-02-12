## README
The following example demonstrates how we can use yuniql to support raw SQL-based migrations in a Entity Framework Core (ef-core) code-first project. 

In this sample, we have standard ef-core code-first project with models only and dbcontext. Then we generate the first migrations with `ef migrations add InitialCreate`. Then we generate the baseline version of database with `ef migrations script` and place it with yunql's `v0.00` version directory. From here, we run `yuniql run` to execute the migrations with the target database.

To simulate changes, we create a new model class `TestClass` and add it into efcore's `yuniqldbContext`. We create efcore migrations step with `ef migrations add TestClass`. This produces a new migration class in `Migrations` directory. Then we generate the change script with `ef migrations script` and place it in yuniql's `v0.01`. From here, we run `yuniql run` to execute the migrations with the target database. Yuniql knows the least version so it will only execute `v0.01`.


## Install yuniql CLI
https://github.com/rdagumampan/yuniql/wiki/Install-yuniql

```powershell
choco install yuniql --version 0.350.0
```

## Install ef-core CLI

```powershell
#change version to version of dotnet core in your machine
dotnet tool install --global dotnet-ef --version 3.0
```

## Clone samples

```powershell
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql-cli
```

## Prepare db server to use

```powershell
#CTRL+C to return to CLI
docker run -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
SETX YUNIQL_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
```

## Produce ef-core baseline migration script, drop into new yuniql version 0.00 directory

```powershell
cd c:\temp\yuniql-cli\samples\postgresql-efcore-sample
dotnet ef migrations add InitialCreate

md c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db
cd c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db

#initialize standard yuniql project structure
yuniql init

cd c:\temp\yuniql-cli\samples\postgresql-efcore-sample
dotnet ef migrations script -o c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\v0.00\migrate.sql
```

## Run yuniql migrations

```powershell
yuniql run -p c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db --platform postgresql -a
yuniql info --platform postgresql
```

## Test if ef produces anything, we script for TestClass
Simulate code-first migrations by adding a new model and generating updated script.
Open project in Visual Studio, Add a new mode class TestClass and edit DBContext.

```csharp
//add new model class
public partial class TestClass
{
    public string TestId { get; set; }

    public string TestColumn { get; set; }

}

//add in dbcontext class
public virtual DbSet<TestClass> TestClass { get; set; }

//add in OnModelCreating(ModelBuilder modelBuilder) method
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<TestClass>(entity =>
    {
        entity.HasNoKey();
    });

    ...
    ...
}        
```

```powershell
dotnet ef migrations add TestClass
```

Run this batch in Poweshell ISE or create a `migrate.ps1` file

```powershell
dotnet ef migrations list >> current_efversions.txt
Get-Content -Path ".\current_efversions.txt"

$fromVersion = Get-Content -Path ".\current_efversions.txt" | Select-Object -Skip 1 | Select-Object -Last 2 | Select-Object -First 1
Write-Host "Scripting migration from: $fromVersion"
dotnet ef migrations script $fromVersion  -o c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\v0.01\migrate.sql
Remove-Item .\current_efversions.txt
```

efcore will generate this migration script and placed inside yuniql's v0.01 directory
```sql
CREATE TABLE "TestClass" (
    "TestId" text NULL,
    "TestColumn" text NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200205212941_TestClass', '3.1.1');
```

```poweshell
yuniql run -p c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db --platform postgresql
yuniql info --platform postgresql

INF   2020-02-05T22:06:08.9588384Z   Started migration from c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db.
INF   2020-02-05T22:06:09.0083410Z   No explicit target version requested. We'll use latest available locally v0.01 ...
INF   2020-02-05T22:06:09.8046388Z   Found the 0 script files on c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\_pre
INF   2020-02-05T22:06:09.8166829Z   Executed script files on c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\_pre
INF   2020-02-05T22:06:09.8263149Z   Found the 1 script files on c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\v0.01
INF   2020-02-05T22:06:09.9152966Z   Executed script file c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\v0.01\migrate.sql.
INF   2020-02-05T22:06:09.9266480Z   Found the 0 bulk files on c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\v0.01
INF   2020-02-05T22:06:09.9448553Z   Completed migration to version c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\v0.01
INF   2020-02-05T22:06:09.9509962Z   Found the 0 script files on c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\_draft
INF   2020-02-05T22:06:09.9647815Z   Executed script files on c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\_draft
INF   2020-02-05T22:06:09.9713872Z   Found the 0 script files on c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\_post
INF   2020-02-05T22:06:09.9857104Z   Executed script files on c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\_post
```

## Cleanup

```powershell
docker ps
docker rm <container-id> -f
```

## References
https://docs.microsoft.com/en-gb/ef/core/get-started/?tabs=netcore-cli
https://www.entityframeworktutorial.net/efcore/create-model-for-existing-database-in-ef-core.aspx
https://www.entityframeworktutorial.net/efcore/cli-commands-for-ef-core-migration.aspx
