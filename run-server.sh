#!/bin/bash
ProjectPath="src/AlternativeMkt/Core"
WebComponentsPath="$ProjectPath/wwwroot/WebComponents"

AdminItemsPageJs=`ls $WebComponentsPath/admin-items-page | grep .js`
echo "Admin items page index js: $AdminItemsPageJs"
PriceDashboardJs=`ls $WebComponentsPath/price-dashboard | grep .js`
echo "Price dashboard index js: $PriceDashboardJs"

export PriceDashboardJs
export AdminItemsPageJs

cd $ProjectPath

echo "Dev: $Dev"

if [ -z $Dev ]; then
	dotnet run
else
	dotnet watch
fi
