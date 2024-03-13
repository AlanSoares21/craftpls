#!/bin/bash

if [ -z $DbUser ]; then
	echo "You should provide the database username"
	exit 1
fi
if [ -z $DbPassword ]; then
	echo "You should provide the database password"
	exit 1
fi
if [ -z $DbHost ]; then
	echo "You should provide the database hostname"
	exit 1
fi
if [ -z $DbPort ]; then
	echo "You should provide the database port"
	exit 1
fi
if [ -z $DbName ]; then
	echo "You should provide the database name"
	exit 1
fi
if [ -z $DockerNetwork ]; then
	echo "You should provide the docker network"
	exit 1
fi


ProjectPath="./src/AlternativeMkt.Db"
ProjectInContainer="/code"
PackagesPath="/root/.nuget/packages"
ToolsPath="/root/.dotnet/tools"

docker run \
--rm -i \
--network $DockerNetwork \
-v $ProjectPath:$ProjectInContainer \
-v dotnet-packages-cache:$PackagesPath \
-v dotnet-tools-cache:$ToolsPath \
-e "ConnectionStrings:MainDb"="User ID=$DbUser;Password=$DbPassword;Host=$DbHost;Port=$DbPort;Database=$DbName" \
mcr.microsoft.com/dotnet/sdk:8.0 << EOF   
pwd
env
cd code
pwd
dotnet restore
dotnet tool install --global dotnet-ef --version 7.0.15
export PATH="$PATH:/root/.dotnet/tools"
dotnet ef database update
EOF
