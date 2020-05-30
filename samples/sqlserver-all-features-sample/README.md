### Database migration project
This database migration project is created and to be executed thru `yuniql`. 
For more how-to guides, samples and developer guides, walk through our [documentation](https://yuniql.io/docs) and bookmark [https://yuniql.io](https://yuniql.io).

#### Run this migration with yuniql on docker

Run with basic parameters
```
docker build -t yuniql-sqlserver-sample .
docker run yuniql-sqlserver-sample -c "your-connection-string"
```

Run with token replacement, verify in `VwVisitorTokenized`
```
docker run yuniql-sqlserver-sample -c "your-connection-string" -k "VwColumnPrefix1=App1,VwColumnPrefix2=App2,VwColumnPrefix3=App3,VwColumnPrefix4=App4"
```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
