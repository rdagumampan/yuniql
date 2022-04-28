#!/bin/bash
if [ $EUID != 0 ]; then
    sudo "$0" "$@"
    exit $?
fi

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
echo "Present Working Directory is $SCRIPT_DIR"

echo "-------- Preparing environment variables"
export YUNIQL_TEST_PLATFORM="mysql"
export YUNIQL_TEST_CONNECTION_STRING="Server=localhost;Port=3306;Database=yuniqldb;Uid=root;Pwd=P@ssw0rd!;"
export YUNIQL_TEST_SAMPLEDB="$SCRIPT_DIR/../../samples/basic-mysql-sample"
export YUNIQL_TEST_CLI="$SCRIPT_DIR/../../yuniql-cli/bin/release/net6.0/linux-x64/publish"
export YUNIQL_TEST_HOST="LOCAL"

echo "-------- Provisioning test database on docker container"
docker rm mysql -f
docker run -dit --name mysql -e "MYSQL_ROOT_PASSWORD=P@ssw0rd!" -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password

echo "-------- Sleep for 30 seconds, waiting for db container to be ready"
sleep 30

echo "-------- Building latest yuniql CLI"
cd ../../yuniql-cli || exit
dotnet build
dotnet publish -c release -r linux-x64 /p:publishsinglefile=true /p:publishtrimmed=true

echo "-------- Running unit tests and platform tests"
cd ../yuniql-tests/platform-tests || exit
dotnet build
dotnet test -v n
