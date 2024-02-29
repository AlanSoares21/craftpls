#!/bin/bash

docker run --rm -v ./:/file \
--network $Network \
-e DbName=$DbName \
-e DbHost=$DbHost \
-e PG_USER=$DbUser \
-e PG_PASSWORD=$DbPassword \
-i postgres /bin/bash << EOF
cd file
echo "$DbHost:5432:$DbName:$DbUser:$DbPassword" > ~/.pgpass
chmod 600 ~/.pgpass
psql $DbName --host=$DbHost --username=$DbUser < pg-dump-2024-02-27.psql
EOF
