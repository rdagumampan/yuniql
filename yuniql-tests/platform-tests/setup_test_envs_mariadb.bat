SETX YUNIQL_TEST_TARGET_PLATFORM "mariadb"
SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost;Port=3306;Database=yuniqldb;Uid=root;Pwd=P@ssw0rd!;"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql-dev\samples\basic-mysql-sample"

SETX YUNIQL_TEST_CLI "C:\play\yuniql-dev\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"

docker run --name mariadb -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mariadb:latest --default-authentication-plugin=mysql_native_password

@echo off
pause