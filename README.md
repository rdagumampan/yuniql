# yuniql ![yuniql-build-status](https://ci.appveyor.com/api/projects/status/e6hqrhqa6d1lnma0?svg=true) [![AppVeyor tests (branch)](https://img.shields.io/appveyor/tests/rdagumampan/yuniql)](https://ci.appveyor.com/project/rdagumampan/yuniql/build/tests) [![Gitter](https://img.shields.io/gitter/room/yuniql/yuniql)](https://gitter.im/yuniql/yuniql) [![Stack Overflow](https://img.shields.io/badge/stack%20overflow-yuniql-green.svg)](http://stackoverflow.com/questions/tagged/yuniql) [![Download latest build](https://ci.appveyor.com/api/projects/status/32r7s2skrgm9ubva?svg=true&passingText=Download%20nightly-win-x64)](https://ci.appveyor.com/api/projects/rdagumampan/yuniql/artifacts/yuniql-nightly.zip)

*** Disclaimer: Yuniql is not yet officially released. Much of the claims here are still work in progress but nightly build have major features available.

yuniql is a database schema versioning and migration tool based on plain sql scripts. Scripts are stored raw `.sql` files in version folders so you can leverage full power of `git`. It automates what you would normally do by hand by executing all scripts in orderly and transactional fashion. Yuniql seamlessly integrates with your Continuous Delivery (CD) pipelines for truely DB DevOps development experience. yuniql lets you take full control of your db schema evolution.

yuniql is released as self-contained .NET Core application (no need for JVM or .NET CLR) for Windows. Container images are also available for seamless Continuous Delivery of database changes in Linux.

<img align="right" src="yuniql-logo.png">

#### Features
- pure sql
- devops pipelines ready
- docker containier ready
- bulk import csv files
- cross platform ***
- enxtensible ***

*** in progress

#### Getting started

1. [How yuniql works](docs/01 how-yuniql-works.md)
2. [Getting started](docs/02 getting-started.md)
3. [How to baseline your database](docs/03 how-to-baseline-your-database.md)
4. [How to use token replacement](docs/04 how-to-use-token-replacement.md)
5. [How to bulk import data during migration](05 how-to-bulk-load-data-during-migration.md)
6. [Best practices](docs/06 best-practices)

#### Supported `yuniql` CLI commands
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
yuniql run -k "Token1=TokenValue1,Token2=TokenValue2,Token3=TokenValue3"
yuniql -v | --version
yuniql -h | --help
yuniql -d | --debug
```

#### Todo

- default cs to env var when yunql run
- execute all subdirectories
- base docker image
- yuniql baseline
- yuniql rebase
- yuniql erase
- version plugins
- nuget package
- az devops tasks
- global dotnet tool

#### How to contribute or ask help
- File a bug or feature as an [issue](https://github.com/rdagumampan/yuniql/issues/new)
- Talk to us on [gitter chat](https://gitter.im/yuniql/community)
- Comment on existing issues and suggest how they should be fixed/implemented
- Fix a bug or implement feature by subimitting a PR
- Write more tests to increase our coverage
- Tag [#yuniql](https://twitter.com/) on Twitter

#### License
TBA
