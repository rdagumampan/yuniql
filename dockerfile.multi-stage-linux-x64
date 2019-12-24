FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

WORKDIR /code
COPY ./yuniql-cli ./yuniql-cli
COPY ./yuniql-core ./yuniql-core
COPY ./yuniql-extensibility ./yuniql-extensibility
COPY ./yuniql-sqlserver ./yuniql-sqlserver
COPY ./yuniql-plugins ./yuniql-plugins

WORKDIR /code/yuniql-cli
RUN dotnet publish -c release -r linux-x64 /p:publishtrimmed=true -o ./app

WORKDIR /code/yuniql-plugins/postgresql/src
RUN dotnet publish -c release -r linux-x64 -o ./app

WORKDIR /code/yuniql-plugins/mysql/src
RUN dotnet publish -c release -r linux-x64 -o ./app

#https://github.com/dotnet/dotnet-docker/issues/1332
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime-env

WORKDIR app
COPY --from=build-env /code/yuniql-cli/app .
COPY --from=build-env /code/yuniql-plugins/postgresql/src/app ./.plugins/postgresql
COPY --from=build-env /code/yuniql-plugins/mysql/src/app ./.plugins/mysql

ENTRYPOINT ["dotnet", "yuniql.dll", "run", "-p", "db"]
CMD ["additional-arguments-captured-from-docker-run"]