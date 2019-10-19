docker-compose down

docker-compose build --no-cache
#docker-compose build

docker-compose up -d

docker exec -it baseliner_db_1 /opt/mssql-tools/bin/sqlcmd -S localhost `
   -U SA -P "P@ssw0rd!" `
   -Q "RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/backup/aw.bak'"

docker exec -it baseliner_db_1 /opt/mssql-tools/bin/sqlcmd `
   -S localhost -U SA -P "P@ssw0rd!" `
   -Q "RESTORE DATABASE AdventureWorks FROM DISK = '/var/opt/mssql/backup/aw.bak' WITH MOVE 'AdventureWorksLT2012_Data' TO '/var/opt/mssql/data/AdventureWorksLT2012.mdf', MOVE 'AdventureWorksLT2012_Log' TO '/var/opt/mssql/data/AdventureWorksLT2012_Log.ldf'"

docker exec -it baseliner_app_1 bash docker run 

#docker exec -it baseliner_db_1 /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "P@ssw0rd!" -Q "RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/backup/aw.bak'"
#docker exec -it baseliner_db_1 /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "P@ssw0rd!" -Q "RESTORE DATABASE AdventureWorks FROM DISK = '/var/opt/mssql/backup/aw.bak' WITH MOVE 'AdventureWorks_Data' TO '/var/opt/mssql/data/AdventureWorksLT2012.mdf', MOVE 'AdventureWorks_Log' TO '/var/opt/mssql/data/AdventureWorksLT2012_Log.ldf'"
