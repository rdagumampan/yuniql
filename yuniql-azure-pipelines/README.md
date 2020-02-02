# Yuniql Azure Pipelines Tasks

Run database migrations and schema versioning with Yuniql. Supports SqlServer, PostgreSql, MySql and others. For samples and developer guide, visit [https://yuniql.io](https://yuniql.io) and walk through [developer wiki](https://github.com/rdagumampan/yuniql/wiki).

**NOTE: This is a PREVIEW RELEASE. Please STAR or WATCH in GitHub for release updates. See https://github.com/rdagumampan/yuniql**

### Azure DevOps YAML Pipelines

``` yaml
trigger:
- master

pool:
  vmImage: 'windows-latest'

steps:
- task: UseYuniqlCLI@0
  inputs:
    version: 'latest'

- task: RunYuniqlCLI@0
  inputs:
    version: 'latest'
    connectionString: 'Server=tcp:<AZ-SQLSERVER>,1433;Initial Catalog=<AZ-SQLDB>;User ID=<USERID>;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
    workspacePath: '$(Build.SourcesDirectory)\sqlserver-samples\visitph-db'
    targetPlatform: 'SqlServer'
    additionalArguments: '--debug'
```

### Use Yuniql Task

![](images/screenshot-01.png)

This downloads and installs the yuniql-cli.
* `version`: The version of Yuniql CLI. If omitted, the latest version of yuniql-cli is installed. Visit the [releases](https://github.com/rdagumampan/yuniql/releases) to get an appropriate version. 

### Run Yuniql Task

![](images/screenshot-02.png)

This runs database migration with yuniql-cli.
* `version`: The version of Yuniql CLI. If omitted, the latest version of yuniql-cli is installed. Visit the [releases](https://github.com/rdagumampan/yuniql/releases) to get an appropriate version. 
* `connectionString`: The connection string to your target database server.
* `workspacePath`: The location of your version directories to run.
* `targetPlatform`: The target database platform. Default is SqlServer.
* `autoCreateDatabase`: When true, creates and configure the database in the target server for yuniql migrations.
* `targetVersion`: The maximum target database schema version to run to.
* `tokenKeyValuePair`: Token key/value pairs for token replacement.
* `additionalArguments`: Additional CLI arguments

### License
Copyright (C) 2019 Rodel E. Dagumampan

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).