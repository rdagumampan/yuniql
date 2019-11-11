FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

WORKDIR /code

#copy local files into app or clone github repo
COPY ./yuniql-cli ./yuniql-cli
COPY ./yuniql-core ./yuniql-core
COPY ./yuniql-extensibility ./yuniql-extensibility
COPY ./yuniql-sqlserver ./yuniql-sqlserver

COPY ./yuniql-tests ./yuniql-tests

#RUN git clone https://github.com/rdagumampan/yuniql.git /code

#build and run integration tests
WORKDIR /code/yuniql-cli
RUN ls
WORKDIR /code/yuniql-tests
RUN ls

RUN dotnet restore
RUN dotnet build