## Yuniql-based Database Migration Project
This database migration project is created and to be executed thru `yuniql`. 
For more how-to guides and deep-divers, please visit yuniql [wiki page on github](https://github.com/rdagumampan/yuniql/wiki).

## Run this migration with yuniql on docker
Open command prompt in current folder.

For simplified run
```
docker build -t <your-project-name> .
docker run your-project-name -c ""<your-connection-string>""
```

For running with token replacement
```
docker run <your-project-name> -c ""<your-connection-string>\"" -k \""<Token1=TokenValue1,Token2=TokebValue2,Token3=TokenValue3,Token4=TokenValue4\>""
```

## How does this works?
When you call `docker build`, we pull the base image containing the nightly build of `yuniql` and all of your local structure is copied into the image. When you call `docker run`, `yuniql run` is executed internally on your migration directory.

>NOTE: The container must have access to the target database. You may need to configure a firewall rule to accept login requests from the container hosts esp for cloud-based databases.


## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
