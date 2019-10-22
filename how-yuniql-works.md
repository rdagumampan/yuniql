### How yuniql works

Yuniql is a schema versioning and database migration tool for sql server. It follows pure sql-based approach to manage schema evolution. This means all scripts are placed transparently as `.sql` files and versions are arrange as series of ordinary directories. Scripts are executed in the same order as they are list by your OS. The most typical work flow would be:
- `yuniql init`
- `yuniql vnext`
- `yuniql run`
- `yuniql info`

#### yuniql init
When you run `yuniql init`, baseline directory structure will be created automatically: 
- *_init*: Initialization scripts. Executed once. This is called when you first do `yuniql init`
- *_pre*: Pre migration scripts. Executed every time before any version. 
- *v0.00*: Migration folder. Executed once.
- *_draft*: Scripts that have not been approved. Executed every time after the latest version.
- *_post*: Post migration scripts. Executed every time and the last to run.
- *Dockerfile*: A template docker file to run your migration with a base docker container with `yuniql` installed.
- *README.md*: A template README file.
- *.gitignore*: A template git ignore file to skip yuniql.exe from being committed.

#### yuniql vnext
When you run `yuniql vnext`, it identifies the latest version locally and increment the minor version with the format `v{major}.{minor}`. The command just helps reduce human errors but you can also create any version in the same format manually. 

There are also advanced options such as `yuniql vnext -M` that increments major version and `yuniql vnext -f initialize_tables.sql` that creates a template file in the created version directory.

#### yuniql run
When you run `yuniql run` the first time, inspects the target database and creates required table to track the versions. All script files in `_init` directory will be executed. The order of execution is as follows `_init`,`_pre`,`vx.xx`,`_draft`,`_post`. Several variations on how can run migration are listed below.

`yuniql run -a`
- Uses connection string from env variable `YUNIQL_CONNECTION_STRING`
- Auto-create target database
- Runs migration

`yuniql run -c "<connectiong-string>"`
- Runs migration using the specified connectiong string

`yuniql run -p c:\temp\demo`
- Runs migration from target directory

`yuniql run -t v1.05`
- Runs migration only upto the version v1.-05

`yuniql run -k "Token1=TokenValue1,Token2=TokenValue2,Token3=TokenValue3"`
- Replace each tokens in each script file
  
#### yuniql info

`yuniql info` shows all version currently present in the target database

### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
