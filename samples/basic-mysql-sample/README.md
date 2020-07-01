# basic-mysql-sample

This project is created and to be executed thru `yuniql` CLI tool. 
- To install `yuniql cli`, see https://yuniql.io/docs/install-yuniql/
- To format connection string, see https://www.connectionstrings.com.

Deploy local postgresql with docker or use your own server

  ```console
  docker run -dit --name mysql -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password
  SETX YUNIQL_CONNECTION_STRING "Server=localhost;Port=3306;Database=helloyuniql;Uid=root;Pwd=P@ssw0rd!;"
  ```

Run migrations

  ```console
  yuniql run -a --platform mysql
  ```

Show migrations

  ```console
  yuniql list --platform mysql
  ```

Erase database

  ```console
  yuniql erase -f --platform mysql
  ```

## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
