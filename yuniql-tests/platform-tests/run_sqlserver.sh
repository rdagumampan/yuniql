#!/bin/bash
if [ $EUID != 0 ]; then
    sudo "$0" "$@"
    exit $?
fi

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
echo "Present Working Directory is $SCRIPT_DIR"

echo "-------- Preparing environment variables"
export YUNIQL_TEST_PLATFORM="sqlserver"
export YUNIQL_TEST_CONNECTION_STRING="Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!"
export YUNIQL_TEST_SAMPLEDB="$SCRIPT_DIR/../../samples/basic-sqlserver-sample"
export YUNIQL_TEST_CLI="$SCRIPT_DIR/../../yuniql-cli/bin/release/net6.0/linux-x64/publish"
export YUNIQL_TEST_HOST="LOCAL"

echo "-------- Provisioning test database on docker container"
docker rm sqlserver -f
docker run -dit --name sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2019-latest

echo "-------- Sleep for 15 seconds, waiting for db container to be ready"
sleep 15

echo "-------- Building latest yuniql CLI"
cd ../../yuniql-cli || exit
dotnet build
dotnet publish -c release -arc x64 -os linux -p:publishsinglefile=true -p:publishtrimmed=true -p:PublishReadyToRun=true

echo "-------- Running unit tests and platform tests"
cd ../yuniql-tests/platform-tests || exit
dotnet build
dotnet test -v n
