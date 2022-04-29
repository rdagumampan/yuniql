#!/bin/bash
if [ $EUID != 0 ]; then
    sudo "$0" "$@"
    exit $?
fi

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
echo "Present Working Directory is $SCRIPT_DIR"

echo "-------- Preparing environment variables"
export YUNIQL_TEST_PLATFORM="sqlserver"
export YUNIQL_TEST_CONNECTION_STRING="Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
export YUNIQL_TEST_SAMPLEDB="$SCRIPT_DIR/../../samples/basic-postgresql-sample"
export YUNIQL_TEST_CLI="$SCRIPT_DIR/../../yuniql-cli/bin/release/net6.0/linux-x64/publish"
export YUNIQL_TEST_HOST="LOCAL"

echo "-------- Provisioning test database on docker container"
docker rm postgresql -f
docker run -dit --name postgresql -e "POSTGRES_USER=sa" -e "POSTGRES_PASSWORD=P@ssw0rd!" -e "POSTGRES_DB=yuniqldb" -p 5432:5432 postgres:latest

echo "-------- Sleep for 30 seconds, waiting for db container to be ready"
sleep 30

echo "-------- Building latest yuniql CLI"
cd ../../yuniql-cli || exit
dotnet build
dotnet publish -c release -arc x64 -os linux -p:publishsinglefile=true -p:publishtrimmed=true -p:PublishReadyToRun=true

echo "-------- Running unit tests and platform tests"
cd ../yuniql-tests/platform-tests || exit
dotnet build
dotnet test -v n
