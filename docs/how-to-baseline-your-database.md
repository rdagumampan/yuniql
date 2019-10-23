### How to baseline your database

A baseline version is your `v0.00` of your database schema. A baseline version helps create full visibility of your schema evolution. We identified two approaches to baselining databases:
1. Visual model first
2. Script first

#### Visual Model First
Typically, we don't start our databases by hand-writing sql scripts, instead we use visual modelling tools such as SSMS, SSDT, IDERA, Sparx EA and similar tools. Especially for larger DW and RDBMS projects, the scripts comes last as a result of good-enough ER model. The scripts are then generated from the tool and this would make a sufficient starting point for baselining the db schema.

In this approach, you can generate all scripts from tool and place all scripts and directories inside `v0.00`. yuniql will discover and execute scripts in all directories and subdirectories.

```
yuniql init
cd v0.00
dir /O:N

10/21/2019  22:41    <DIR>          tables
10/21/2019  22:41    <DIR>          functions
10/21/2019  22:41    <DIR>          stored-procedures
10/21/2019  22:41    <DIR>          views
```

#### Script first
For smaller databases especially those attached to microservices, the model is relatively small and tables can be scripted on the go. Its simple and you can manually place all your scripts in order in 'v0.00'. Scripts are executed in order by file name.

```
yuniql init
cd v0.00
dir /O:N

10/21/2019  22:41                   setup-tables.sql
10/21/2019  22:41                   setup-stored-procedures.sql
10/21/2019  22:41                   initialize-tables.sql
```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
