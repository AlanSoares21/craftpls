#!/bin/bash
if [ -z $ApiUrl ]; then
	echo "The ApiUrl must be provided"
	exit 1
fi
if [ -z $AssetsUrl ]; then
	echo "The AssetsUrl must be provided"
fi
Root=`pwd`
echo "Root: $Root"
PriceDashboardPath="src/WebComponents/price-dashboard"
SiteProjectPath="src/AlternativeMkt"
WebComponentsPath="$SiteProjectPath/wwwroot/WebComponents"
if [ -d $WebComponentsPath ]; then
	echo "Removing previous versions of web components in $WebComponentsPath"
	rm -r "$WebComponentsPath/"
fi	
mkdir $WebComponentsPath

cd $PriceDashboardPath
echo -e "VITE_ApiUrl=$ApiUrl\nVITE_AssetsUrl=$AssetsUrl" > .env.production.local
npm run build
cd $Root
cp -r "$PriceDashboardPath/dist/assets" "$SiteProjectPath/wwwroot/WebComponents/price-dashboard"
