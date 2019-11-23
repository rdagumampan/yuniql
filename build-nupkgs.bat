dotnet pack ./yuniql-extensibility/Yuniql.Extensibility.csproj -p:packageversion=0.2.0 -c release -o nupkgs
dotnet pack ./yuniql-core/Yuniql.Core.csproj -p:packageversion=0.2.0 -c release -o nupkgs
dotnet pack ./yuniql-sqlserver/Yuniql.SqlServer.csproj -p:packageversion=0.2.0 -c release -o nupkgs
dotnet pack ./yuniql-plugins/postgresql/src/Yuniql.PostgreSql.csproj -p:packageversion=0.2.0 -c release -o nupkgs

dotnet nuget push ./nupkgs/Yuniql.Extensibility.0.2.0.nupkg -k APIKEY -s https://api.nuget.org/v3/index.json
dotnet nuget push ./nupkgs/Yuniql.Core.0.2.0.nupkg -k APIKEY -s https://api.nuget.org/v3/index.json
dotnet nuget push ./nupkgs/Yuniql.SqlServer.0.2.0.nupkg -k APIKEY -s https://api.nuget.org/v3/index.json
dotnet nuget push ./nupkgs/Yuniql.PostgreSql.0.2.0.nupkg -k APIKEY -s https://api.nuget.org/v3/index.json
