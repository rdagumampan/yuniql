### Database migration project
This database migration project is created and to be executed thru `yuniql`. 
For more how-to guides and deep-divers, please visit yuniql [wiki page on github](https://github.com/rdagumampan/yuniql/wiki).

#### Run this migration with yuniql on docker

Run with basic parameters
```
docker build -t helloyuniql .
docker run helloyuniql -c "your-connection-string"
```

Run with token replacement, verify in `VwVisitorTokenized`
```
docker run helloyuniql -c "your-connection-string" -k "VwColumnPrefix1=App1,VwColumnPrefix2=App2,VwColumnPrefix3=App3,VwColumnPrefix4=App4"
```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
