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

#### yuniql info

### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
