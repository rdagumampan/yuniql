# README 

This project is just to demonstrate how you can test the plugin in isolation. A reusable platform tests is available in the `yuniql-platformtests` to cover the general behaviour of the migration using plugin coomponents. The instruction on how to run platform test is availabe in README of the `yuniql-platformtests` project.

To start debugging, you would need to a postgresql instance running either locally or on docker container.
	
```console
docker run --name postgresql -e POSTGRES_USER=app -e POSTGRES_PASSWORD=app -e POSTGRES_DB=yuniqldb  -d -p 5432:5432 postgres
```

To view and manage 
```console
docker run --name pgadmin4  -p 80:80 -e "PGADMIN_DEFAULT_EMAIL=admin@getyuniql.org" -e "PGADMIN_DEFAULT_PASSWORD=app" -d dpage/pgadmin4
```

## References

* https://info.crunchydata.com/blog/easy-postgresql-10-and-pgadmin-4-setup-with-docker
* https://www.npgsql.org/doc/index.html
* https://hub.docker.com/_/postgres
* https://www.pgadmin.org/download/pgadmin-4-windows/
