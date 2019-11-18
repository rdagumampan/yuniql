# README 


```
version: '3.4'
services:
  postgres:
    image: postgres
    environment:
      - POSTGRES_USER=app
      - POSTGRES_PASSWORD=app
      - POSTGRES_DB=mydbname
    volumes:
      - ./volumes/data/db:/var/lib/postgresql/data
    ports:
       - 5432:5432

	   docker run -e POSTGRES_USER=app -e POSTGRES_PASSWORD=app -e POSTGRES_DB=mydbname -p 5432:5432 postgres
```

```bash
SETX YUNIQL_TEST_TARGET_PLATFORM "pqsql"
SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=5432;Username=app;Password=app;Database=mydbname"
```

http://www.postgresqltutorial.com/connect-to-postgresql-database/

https://www.pgadmin.org/download/pgadmin-4-windows/

## References

https://www.npgsql.org/doc/index.html

https://hub.docker.com/r/phpmyadmin/phpmyadmin/


Host=localhost;Port=5432;Username=app;Password=app;Database=mydbname
