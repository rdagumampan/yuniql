SETX YUNIQL_TEST_TARGET_PLATFORM "cockroachdb"
SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=26257;Username=root;Password=P@ssw0rd!;Database=yuniqldb"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql-dev\samples\basic-postgresql-sample"

SETX YUNIQL_TEST_CLI "C:\play\yuniql-dev\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"

docker network create -d bridge crdbnet
docker run -d --name=cockroachdb --hostname=crdb --net=crdbnet -p 26257:26257 -p 8080:8080  cockroachdb/cockroach start --insecure

@echo off
pause