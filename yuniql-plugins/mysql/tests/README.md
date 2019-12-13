# README 

This project is just to demonstrate how you can test the plugin in isolation. A reusable platform tests is available in the `yuniql-platformtests` to cover the general behaviour of the migration using plugin coomponents. The instruction on how to run platform test is availabe in README of the `yuniql-platformtests` project.

To start debugging, you would need to a postgresql instance running either locally or on docker container.
	
```console
	docker run --name mysql -e MYSQL_ROOT_PASSWORD=app -d -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password
```

## References
* https://hub.docker.com/r/phpmyadmin/phpmyadmin/
