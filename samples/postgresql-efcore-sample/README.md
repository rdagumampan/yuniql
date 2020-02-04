## README
The following example demonstrates how we can use yuniql to support raw SQL-based migrations in a Entity Framework Core (ef-core) code-first project. 

In this sample, we have standard ef-core code-first project with models only and dbcontext. Then we generate the first migrations with `ef migrations add InitialCreate`. Then we generate the baseline version of database with `ef migrations script` and place it with yunql's `v0.00` version directory. From here, we run `yuniql run` to execute the migrations with the target database.

To simulate changes, we create a new model class `TestClass` and add it into efcore's `yuniqldbContext`. We create efcore migrations step with `ef migrations add TestClass`. This produces a new migration class in `Migrations` directory. Then we generate the change script with `ef migrations script` and place it in yuniql's `v0.01`. From here, we run `yuniql run` to execute the migrations with the target database. Yuniql knows the least version so it will only execute `v0.01`.


## Install yuniql CLI
https://github.com/rdagumampan/yuniql/wiki/Install-yuniql

```console
choco install yuniql --version 0.328.0
```

## Install ef-core CLI

```console
#change version to version of dotnet core in your machine
dotnet tool install --global dotnet-ef --version 3.0
```

## Clone samples

```console
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql-cli
```

## Prepare db server to use

```console
docker run -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
SETX YUNIQL_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
```

## Produce ef-core baseline migration script, drop into new yuniql version 0.00 directory

```console
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

```console
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

```console
dotnet ef migrations add TestClass
dotnet ef migrations script 20200204214045_InitialCreate  -o c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\v0.01\migrate.sql

yuniql run -p c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db --platform postgresql
yuniql info --platform postgresql
```

## References
https://docs.microsoft.com/en-gb/ef/core/get-started/?tabs=netcore-cli
https://www.entityframeworktutorial.net/efcore/create-model-for-existing-database-in-ef-core.aspx
https://www.entityframeworktutorial.net/efcore/cli-commands-for-ef-core-migration.aspx
