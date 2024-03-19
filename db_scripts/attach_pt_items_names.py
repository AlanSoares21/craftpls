from utils import itemNameToPostgresString, readJson
import psycopg2
import os
from dotenv import load_dotenv

def getCultureItemIndex(cultureItem, enItems: list):
    for index in range(len(enItems)):
        item = enItems[index]
        if item['id'] == cultureItem['id']:
            return index
    return -1

def getDbItem(cursor: psycopg2.extensions.cursor, enItem, category: int):
    enName = str(enItem['name'])
    enLevel = int(enItem['level'])
    cursor.execute('select id, name, level, categoryid from craft_items where name = \'' + itemNameToPostgresString(enName) + '\' and level = ' + str(enLevel) + ' and categoryid = ' + str(category))
    row = cursor.fetchone()
    if row == None:
        return None
    return {'id': int(row[0])}

def getItemNameByCulture(cursor: psycopg2.extensions.cursor, itemId: int, culture: str):
    cursor.execute('select id, name from craft_items_data_by_culture where culture = \'' + culture + '\' and itemid = ' + str(itemId))
    row = cursor.fetchone()
    if row == None:
        return None
    return {'id': int(row[0]), 'name': str(row[1])}

def saveCultureNameForItem(cursor: psycopg2.extensions.cursor, itemId: int, culture: str, name: str):
    cursor.execute('insert into craft_items_data_by_culture(itemid, culture, name) values (' + str(itemId) + ', \'' + culture + '\', \'' + name + '\') returning id')
    row = cursor.fetchone()
    if row == None:
        return None
    return {'id': int(row[0]), 'name': name, 'culture': culture, 'itemid': itemId}

def attachNamesToItems(cursor: psycopg2.extensions.cursor, enItems: list, cultureItems: list, culture: str, category: int):
    for cultureItem in cultureItems:
        enIndex = getCultureItemIndex(cultureItem, enItems)
        if enIndex == -1:
            print('Item ' + str(cultureItem['name']) + '(' + str(cultureItem['id']) + ') not found in the en items')
            continue
        enItem = enItems[enIndex]
        enName = str(enItem['name'])
        enLevel = int(enItem['level'])
        dbItem = getDbItem(cursor, enItem, category)
        if dbItem == None:
            print('Item ' + enName + ' level ' + str(enLevel) + ' with category ' + str(category) + ' not found in the db')
            continue
        dataByCulture = getItemNameByCulture(cursor, dbItem['id'], culture)
        if dataByCulture != None:
            print('Item ' + str(dbItem['id']) + ' already has the name ' + dataByCulture['name'] + ' for culture ' + culture +  ' in the db')
            continue
        result = saveCultureNameForItem(cursor, dbItem['id'], culture, str(cultureItem['name']))
        if result != None:
            conn.commit()
            print('success on save name ' + result['name'] + '(' + str(result['id']) + ') with culture ' + result['culture'] + ' for item ' + str(result['itemid']))
        else:
            print('fail on save name ' + str(cultureItem['name']) + ' with culture ' + culture + ' for item ' + str(dbItem['id']))


load_dotenv()

targetCulture = 'pt'
itemTypesToCraftCategory = readJson('./itemTypeToCraftCategory.json')

connString = os.getenv('PG_CONNSTR')
conn = psycopg2.connect(connString)
try:
    cursor = conn.cursor()
    for itemsType in itemTypesToCraftCategory:
        print('check items ' + itemsType)
        cultureItemsFile = readJson('data/' + targetCulture + '/' + itemsType + '.json')['list']
        enItemsFile = readJson('data/' + itemsType + '.json')['list']
        attachNamesToItems(cursor, enItemsFile, cultureItemsFile, targetCulture, int(itemTypesToCraftCategory[itemsType]))
except Exception as err:
    print(err.with_traceback())
finally:
    conn.close()