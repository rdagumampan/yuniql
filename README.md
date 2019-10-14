
# yuniql ![yuniql-build-status](https://ci.appveyor.com/api/projects/status/e6hqrhqa6d1lnma0?svg=true) ![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/rdagumampan/yuniql) [![Gitter](https://img.shields.io/gitter/room/yuniql/yuniql)](https://gitter.im/yuniql/yuniql) [![Download latest build](https://ci.appveyor.com/api/projects/status/32r7s2skrgm9ubva?svg=true&passingText=Download%20latest-win-x64)](https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-nightly.zip)

straight forward sql server database migration
<img align="right" src="yuniql-logo.png">

#### Motivation

- ridiculously simple! you just need to know sql
- true cross-platform, runs on windows and linux
- zero dependencies, self-contained app, no jvm!
- devops ready, use your own version control and pipelines framework
- free and open source, TBA license

#### Features
- pure sql migration
- cross platform releases ***
- devops pipelines ready
- docker container ready
- bulk import csv files
- pluggable versions ***

*** in progress

#### Supported Yuniql CLI commands
```console
yuniql init
yuniql init -p c:\temp\demo | --path c:\temp\demo
yuniql vnext
yuniql vnext -p c:\temp\demo | --path c:\temp\demo
yuniql vnext -M | --major
yuniql vnext -m | --minor
yuniql vnext -f "Table1.sql"
yuniql run
yuniql run -a true | --auto-create-db true
yuniql run -p c:\temp\demo | --path c:\temp\demo
yuniql run -t v1.05 | --target-version v1.05
yuniql run -c "<connectiong-string>"
yuniql -v | --version
yuniql -h | --help
yuniql -d | --debug
```

#### Run from windows (self-contained/.exe/win-x64)
```console
yuniql init
yuniql vnext
yuniql vnext -M
yuniql vnext -a
yuniql run -a
```

#### Run from ubuntu linux
```console
dotnet "yuniql.dll" "run" -p "c:\temp\demo" -a
```

#### Run integration tests on local docker swarm

```console
docker-compose build --no-cache
docker-compose up -d && docker-compose logs -f
docker-compose down
```

#### Getting started
1. Versioning a new database TBA
2. Versioning an existing database TBA
3. Migrating from another tool TBA

#### How to get help
Read the documentation and how-to guides. You may also get in touch via:
- Submit an [issue](https://github.com/rdagumampan/yuniql/issues/new) on GitHub
- Talk to us on [Gitter chat](https://gitter.im/yuniql/community)
- Tag [#yuniql](https://twitter.com/) on Twitter

#### Contributing
Start with submitting an issue request or picking on an issue for PR.

#### License
TBA
