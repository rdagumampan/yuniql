### Database migration project
This database migration project is created and to be executed thru `yuniql`. 
For more how-to guides and deep-divers, please visit yuniql [wiki page on github](https://github.com/rdagumampan/yuniql/wiki).

#### Run this migration with yuniql on docker

Run with basic parameters
```
docker build -t yuniql-mysql-sample .
docker run yuniql-mysql-sample --platform mysql -c "your-connection-string"
```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
