#!/bin/bash

if [ -z $DbUser ]; then 
	echo "Db user must be provided"
	exit 1
fi
if [ -z $DbPassword ]; then
	echo "Db password must be provided"
	exit 1
fi
if [ -z $DbContainer ]; then 
	DbContainer="altmkt-db"
fi
if [ -z $Network ]; then 
	Network="alt-mkt-net"
fi
if [ -z $Port ]; then
	Port=5432
fi

if [ -z $DbName ]; then
	DbName='altmktmaindb'
fi

docker run --rm --network $Network -v ./db-backup:/backup -e PG_USER=$DbUser -e PG_PASSWORD=$DbPassword -i postgres:16 /bin/bash << EOF 
echo "$DbContainer:$Port:$DbName:$DbUser:$DbPassword" > ~/.pgpass
chmod 600 ~/.pgpass
pg_dump $DbName -w --host=$DbContainer --username=$DbUser | gzip > /backup/pg-dump-$(date +%Y-%m-%d).psql.gz 
EOF
