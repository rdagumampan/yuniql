### Database migration project
This database migration project is created and to be executed thru `yuniql`. 
For more how-to guides, samples and developer guides, walk through our [documentation](https://yuniql.io/docs) and bookmark [https://yuniql.io](https://yuniql.io).

#### Run this migration with yuniql on docker

Run with basic parameters
```
docker build -t yuniql-cockroachdb-sample .
docker run yuniql-cockroachdb-sample --platform cockroachdb -c "your-connection-string"
```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
