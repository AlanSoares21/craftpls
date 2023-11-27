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
AdminItemsPagePath="src/WebComponents/admin-items-page"
SiteProjectPath="src/AlternativeMkt/Core"
WebComponentsPath="$SiteProjectPath/wwwroot/WebComponents"
if [ -d $WebComponentsPath ]; then
	echo "Removing previous versions of web components in $WebComponentsPath"
	rm -r "$WebComponentsPath/"
fi	
mkdir $WebComponentsPath

echo "building price dashboard"
cd $PriceDashboardPath
echo -e "VITE_ApiUrl=$ApiUrl\nVITE_AssetsUrl=$AssetsUrl" > .env.production.local
npm run build
PriceDashboardJs=`ls dist/assets | grep .js`
echo "Price dashboard index js name: $PriceDashboardJs"

cd $Root

echo "building admin items page"
cd $AdminItemsPagePath
echo -e "VITE_ApiUrl=$ApiUrl\nVITE_AssetsUrl=$AssetsUrl" > .env.production.local
npm run build
AdminItemsPageJs=`ls dist/assets | grep .js`
echo "Admin items page index js name: $AdminItemsPageJs"

cd $Root

cp -r "$PriceDashboardPath/dist/assets" "$SiteProjectPath/wwwroot/WebComponents/price-dashboard"
cp -r "$AdminItemsPagePath/dist/assets" "$SiteProjectPath/wwwroot/WebComponents/admin-items-page"
