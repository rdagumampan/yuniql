FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

WORKDIR /code

#copy local files into app or clone github repo
COPY *.csproj .
COPY *.sln .
COPY *.cs ./

#RUN git clone https://github.com/rdagumampan/yuniql.git /code

#build and run integration tests
RUN ls

RUN dotnet restore
RUN dotnet build