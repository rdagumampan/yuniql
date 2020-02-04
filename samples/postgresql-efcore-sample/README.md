## README

## Install yuniql
https://github.com/rdagumampan/yuniql/wiki/Install-yuniql

choco install yuniql --version 0.328.0

## Clone samples
git clone https://github.com/rdagumampan/yuniql.git c:\temp\yuniql-cli

## Prepare dev database

docker run -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
SETX YUNIQL_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"

## Install ef-core

#change version to version of dotnet core in your machine
dotnet tool install --global dotnet-ef --version 3.0

## Prepare test ef-core project

cd c:\temp\yuniql-cli\samples\postgresql-efcore-sample

## Produce ef-core baseline migration script, drop into new yuniql version 0.00 directory

dotnet ef migrations add InitialCreate

md c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db
cd c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db
yuniql init

dotnet ef migrations script -o c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db\v0.00\migrate.sql

## Run yuniql migrations

yuniql run -p c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db --platform postgresql
yuniql info --platform postgresql

## Test if ef produces anything, we script for TestClass

//add new model class
public partial class TestClass
{
    public string TestId { get; set; }

    public string TestColumn { get; set; }

}

//add in dbcontext class
public virtual DbSet<TestClass> TestClass { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<TestClass>(entity =>
    {
        entity.HasNoKey();
    });

    ...
    ...
}        

dotnet ef migrations add TestClass
dotnet ef migrations script -i -c yuniqldbContext -o c:\temp\postgresql-efcore-sample\_db\v0.01\migrate.sql

yuniql run -p c:\temp\yuniql-cli\samples\postgresql-efcore-sample\_db --platform postgresql
yuniql info --platform postgresql

References
https://docs.microsoft.com/en-gb/ef/core/get-started/?tabs=netcore-cli
https://www.entityframeworktutorial.net/efcore/create-model-for-existing-database-in-ef-core.aspx
https://www.entityframeworktutorial.net/efcore/cli-commands-for-ef-core-migration.aspx
