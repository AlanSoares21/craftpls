#!/bin/bash

function ifErrorExit() {
	if [ $1 -ne 0 ]; then
		echo $2
		exit $1
	fi
}

# verifica variaveis de ambiente
if [ -z $DbUser ]; then
	echo "You must provide the database username"
	echo $DbUser
	exit 1
fi
if [ -z $DbPassword ]; then
	echo "You must provide the database user password"
	exit 1
fi
if [ -z $GoogleClientId ]; then
	echo "You must provide the google client id"
	exit 1
fi
if [ -z $GoogleSecret ]; then
	echo "You must provide the google secret"
	exit 1
fi
if [ -z $AssetsUrl ]; then
	echo "You must provide the assets url"
	exit 1
fi
if [ -z $SiteUrl ]; then
	echo "You must provide the site url"
	exit 1
fi
if [ -z $JwtSecret ]; then
	echo "You must provide the jwt secret"
	exit 1
fi

DockerNetwork="alt-mkt-net"
# deploy do banco de dados
DbContainerName="altmkt-db"
AdminerContainerName="adminer-cnt"
DbVolumeName="alternative-mkt-db-vol"
DbName="altmktmaindb"
DbPort=5432
DbHost="altmktmaindb"
ConnString="User ID=$DbUser;Password=$DbPassword;Host=$DbHost;Port=$DbPort;Database=$DbName"

# check if the container for db is created
DbContainerId=`docker ps -a -f name=$DbContainerName --format "{{ .ID }}"`
if [ -n "$DbContainerId" ]; then
	# check if the db is not running
	DbContainerId=`docker ps -f name=$DbContainerName --format "{{ .ID }}"`
	if [ -z "$DbContainerId" ]; then
		# restart db container
		docker restart $DbContainerName
		ifErrorExit $? "Error on restart database container"
	fi
	AdminerContainerId=`docker ps -f name=$AdminerContainerName --format "{{ .ID }}"`
	if [ -z "$AdminerContainerId" ]; then
		# restart adminer container
		docker restart $AdminerContainerName
		ifErrorExit $? "Error on restart adminer container"
	fi

else
	# run db container
	docker volume create $DbVolumeName
	ifErrorExit $? "Error creating the database volume"
	docker run -dp 3242:5432 --name $DbContainerName --hostname $DbHost -v $DbVolumeName:/var/lib/postgresql/data --network $DockerNetwork -e PGDATA=/var/lib/postgresql/data/altmkt -e POSTGRES_DB=$DbName -e POSTGRES_USER=$DbUser -e POSTGRES_PASSWORD=$DbPassword postgres 
	ifErrorExit $? "Error running the database"
	docker run -dp 4080:8080 --name $AdminerContainerName --network $DockerNetwork adminer
fi

echo "Database deployed"

echo "building spa"
# build dos spa's
export ApiUrl="$SiteUrl/api"
./build-web-components.sh
ifErrorExit $? "Error building spa's components"

cd src
# get spa js name
AdminItemPageJs=`ls AlternativeMkt/Core/wwwroot/WebComponents/admin-items-page | grep .js`
PriceDashboardJs=`ls AlternativeMkt/Core/wwwroot/WebComponents/price-dashboard | grep .js`

echo "finished spa build"

# build do site
AltMktImg="alternative-mkt"
echo "building site image"

docker build . -t $AltMktImg --file "altmkt.Dockerfile" --force-rm
ifErrorExit $? "Error building site image"
echo "end build site image"

# config server env variables

echo "creating site config"
echo -e "Roles:Dev=2\nRoles:Admin=1\nJwt:Secret=$JwtSecret\nJwt:SecondsAuthTokenExpire=60\nJwt:Issuer=$SiteUrl\nJwt:Audience=$SiteUrl\nJwt:AllowedOrigin=$SiteUrl\nGoogle:ClientSecret=$GoogleSecret\nGoogle:ClientId=$GoogleClientId\nClientOrigin=$SiteUrl\nConnectionStrings:MainDb=$ConnString\nPriceDashboardJs=$PriceDashboardJs\nAdminItemPageJs=$AdminItemsPageJs\nAssetsUrl=$AssetsUrl" > .env.production

AltMktContainerName="altmkt-site"
AltMktHost="altmkt"

# check if the site container exists
AltMktContainerId=`docker ps -a -f name=$AltMktContainerName --format "{{ .ID }}"`
if [ -n "$AltMktContainerId" ]; then
	# stop the container
	docker stop $AltMktContainerName
	ifErrorExit $? "Error stopping old site container"
	docker rm $AltMktContainerName
	ifErrorExit $? "Error removing old site container"
	docker image prune
	ifErrorExit $? "Error on running docker prune after remove site old container"
fi

echo "running site container"
docker run -dp 4000:80 --name $AltMktContainerName --network $DockerNetwork --hostname $AltMktHost --env-file .env.production $AltMktImg 
ifErrorExit $? "Error on running the site container"
echo "site deployed"
