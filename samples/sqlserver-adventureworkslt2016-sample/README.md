### Database migration project
This database migration project is created and to be executed thru `yuniql`. 
For more how-to guides, samples and developer guides, walk through our [documentation](https://yuniql.io/docs) and bookmark [https://yuniql.io](https://yuniql.io).

#### Run this migration with yuniql on docker
Open command prompt in current folder.

For simplified run
```
docker build -t adw-example .
docker run adw-example -c "your-connection-string"
```

For running with token replacement
```
docker run adw-example -c "your-connection-string\" -k \"VwColumnPrefix1=App1,VwColumnPrefix2=App2,VwColumnPrefix3=App3,VwColumnPrefix4=App4\"
```

#### Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
