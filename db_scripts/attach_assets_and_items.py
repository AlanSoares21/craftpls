import json
from types import TracebackType
import psycopg2
import os
from dotenv import load_dotenv

from utils import itemNameToPostgresString, readJson

iconsMapFilePath = 'data/icons' + '/iconsMap.json' 

iconsMap = readJson(iconsMapFilePath)

itemsFromFile = []

itemsTypes = readJson('./itemsTypes.json')

for t in itemsTypes:
    filename = 'data/' + t + '.json'
    try:
        itemsFromFile.extend(readJson(filename)['list'])
    except Exception as err:
        print('Error on reading file ' + filename)
        print(err.with_traceback())
        exit()

print(str(len(itemsFromFile)) + ' items from json files')

load_dotenv()
connString = os.getenv('PG_CONNSTR')
conn = psycopg2.connect(connString)

# some items are duplicated in level and in name
# but the craft type is different so is not the same item
# this list identifyes this items
duplicatedItems = []

try:
    cursor = conn.cursor()

    # cursor.execute("select id, endpoint from assets")

    # dbAssets = cursor.fetchall()
    # print(str(len(dbAssets)) + ' assets found in the database')

    # link assets and items
    for item in itemsFromFile:
        iconIdStr = str(item['icon'])
        iconSha = iconsMap[iconIdStr]
        print('For ' + iconIdStr + ' found sha ' + iconSha)
        cursor.execute('select id from assets where endpoint = \'' + iconSha + '.webp\'')
        assetId = cursor.fetchone()
        if assetId is None:
            print('Error not found asset for icon ' + iconIdStr + ' with sha ' + iconSha)
            break
        itemName = itemNameToPostgresString(str(item['name']))
        itemLevel = str(int(item['level']))
        print('Attaching asset for item ' + itemName + ' level ' + itemLevel)
        cursor.execute('update craft_items set assetid = ' + str(assetId[0]) + ' where name = \'' + itemName + '\' and level = ' + itemLevel)
        if cursor.rowcount > 1:
            print('Rows affected after update is ' + str(cursor.rowcount) + ' on attaching asset for item ' + itemName + ' level ' + itemLevel + '\n item added in duplicated list')
            duplicatedItems.append(item)
        if cursor.rowcount is 0:
            print('Rows affected after update is ' + str(cursor.rowcount) + ' on attaching asset for item ' + itemName + ' level ' + itemLevel)
            exit()
        conn.commit()
except Exception as err:
    print(err.with_traceback())
finally:
    conn.close()

print(str(len(duplicatedItems)) + ' items with the same name and the same level')

with open('sameNameAndLevel.json', 'w') as file:
    file.truncate()
    json.dump(duplicatedItems, file, indent=4)
