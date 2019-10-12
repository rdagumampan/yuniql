FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

#copy local files into app or clone github repo
WORKDIR /code
RUN git clone https://github.com/rdagumampan/yuniql.git /code

#build
WORKDIR /code/src
RUN ls
RUN dotnet restore
RUN dotnet publish -c release -r linux-x64 /p:publishsinglefile=true /p:publishtrimmed=true -o /app

#FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime-env
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS runtime-env
WORKDIR /app
COPY --from=build-env /app ./
RUN ls

ENTRYPOINT ["dotnet", "yuniql"]