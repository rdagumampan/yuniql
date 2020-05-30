SETX YUNIQL_TEST_TARGET_PLATFORM "postgresql"
SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql-dev\samples\basic-postgresql-sample"

SETX YUNIQL_TEST_CLI "C:\play\yuniql-dev\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"

docker run --name yuniql-postgresql -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=helloyuniql -p 5432:5432 postgres

@echo off
pause