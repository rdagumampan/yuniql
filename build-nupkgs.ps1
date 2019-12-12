param(
    [string]$version,
    [string]$apiKey
)

dotnet pack ./yuniql-core/Yuniql.Core.csproj -p:packageversion=$version -c release -o nupkgs
dotnet pack ./yuniql-extensibility/Yuniql.Extensibility.csproj -p:packageversion=$version -c release -o nupkgs

dotnet nuget push ./nupkgs/Yuniql.Core.$version.nupkg -k $apiKey -s https://api.nuget.org/v3/index.json
dotnet nuget push ./nupkgs/Yuniql.Extensibility.$version.nupkg -k $apiKey -s https://api.nuget.org/v3/index.json

dotnet pack ./yuniql-sqlserver/Yuniql.SqlServer.csproj -p:packageversion=$version -c release -o nupkgs
dotnet nuget push ./nupkgs/Yuniql.SqlServer.$version.nupkg -k $apiKey -s https://api.nuget.org/v3/index.json