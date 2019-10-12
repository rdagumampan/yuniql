FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

WORKDIR /code

#copy local files into app or clone github repo
COPY ./src ./src/
COPY ./tests ./tests/

#RUN git clone https://github.com/rdagumampan/yuniql.git /code

#build and run integration tests
WORKDIR /code/src
RUN ls
WORKDIR /code/tests
RUN ls

RUN dotnet restore
RUN dotnet build