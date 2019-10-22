### How to use yuniql token replacement

A series of key/value pairs of tokens can be passed to `yuniql`. During migration run, yuniql inspects all script files and replaces them. This is particulary useful in cases such as cross-database and linked-server queries where the databases and server names varies per environment.

The following script would failed when run in TEST where `EMPLOYEEDB_DEV` database does not exists but `EMPLOYEEDB_TEST`.
```sql
SELECT E.FirstName, E.LastName, E.Address, E.Email 
FROM [EMPLOYEEDB_DEV].[dbo].[Employee] E 
ORDER BY E.FirstName ASC
```

To resolve this, pre-pare your script with token `%{ENV_EMPLOYEEDB}`. You can use whatever token name.
```sql
SELECT E.FirstName, E.LastName, E.Address, E.Email 
FROM EMPLOYEEDB_%{ENV-DBNAME-SUFFIX}.[dbo].[Employee] E 
ORDER BY E.FirstName ASC
```

Pass the tokens when you run migration

```
yuniql run -k "ENV-DBNAME-SUFFIX=DEV" -c "<you-dev-connection-string>"
yuniql run -k "ENV-DBNAME-SUFFIX=TEST" -c "<you-test-connection-string>"
yuniql run -k "ENV-DBNAME-SUFFIX=PROD" -c "<you-prod-connection-string>"
```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
