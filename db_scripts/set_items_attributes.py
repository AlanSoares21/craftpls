import json
import time
import psycopg2
from dotenv import load_dotenv
import os
from utils import itemNameToPostgresString, readJson
import requests as rt

load_dotenv()

itemTypeToCraftCategory = readJson('itemTypeToCraftCategory.json')

itemsJsonfolderPath = "data/"
filenames = [x for x in os.listdir(itemsJsonfolderPath) if x.endswith('.json')]
items = []
print("amount files: " + str(len(filenames)))
for filename in filenames:
    file = readJson(itemsJsonfolderPath + filename)
    items.extend(file['list'])

print("amount items: " + str(len(items)))


connString = os.getenv('PG_CONNSTR')
conn = psycopg2.connect(connString)

wsDbUrl = 'https://wsdb.xyz/api/data'

itemsCachePath = './data/items_cache'
if not os.path.exists(itemsCachePath):
    os.mkdir(itemsCachePath)
itemsInCache = [int(item[:-5]) for item in os.listdir(itemsCachePath) if item.endswith('.json')]

def getItemData(wsDbItemId: int):
    try:
        itemCacheFileName = itemsCachePath + "/" + str(wsDbItemId) + ".json"
        if wsDbItemId in itemsInCache:
            return readJson(itemCacheFileName)
        data = rt.get(wsDbUrl + '/item/en/' + str(wsDbItemId)).json()
        with open(itemCacheFileName, 'w') as file:
            file.truncate()
            json.dump(data, file, indent=4)
        return data
    except Exception as err:
        print(err)
        return None

def getDbAttribtues(cursor: psycopg2.extensions.cursor):
    cursor.execute("select Id, name from attributes")
    rows = cursor.fetchall()
    attributes = [{'id': int(row[0]), 'name': str(row[1])} for row in rows]
    return attributes

def getDbItem(cursor: psycopg2.extensions.cursor, itemJsonData):
    cursor.execute("select id, name, level from craft_items where name like '" + itemNameToPostgresString(str(itemJsonData['name'])) + "' and level = " + str(int(itemJsonData['level'])))
    data = cursor.fetchone()
    if data == None:
        return None
    item = {'id': int(data[0]), 'name': str(data[1])}
    if data[2] != None:
        item['level'] = int(data[2])
    cursor.execute("select cia.id, cia.value, at.id, at.name  from craft_items_attributes as cia inner join attributes as at on at.id = cia.attributeid where cia.itemid = " + str(item['id']))
    rows = cursor.fetchall()
    item['attributes'] = [{'ciaid': row[0], 'value': float(row[1]), 'name': str(row[3]), 'atid': int(row[2])} for row in rows]
    return item

def addAttribute(cursor: psycopg2.extensions.cursor, name: str):
    cursor.execute("insert into attributes(name) values ('" + name + "') returning id")
    data = cursor.fetchone()
    if data == None:
        return None
    att = {'id': int(data[0]), 'name': name}
    return att

def findAttribute(dbAttributes, attributeName: str):
    for att in dbAttributes:
        if att['name'] == attributeName:
            return att
    return None

def craftItemHasThisAttribute(item, attributeName: str):
    for att in item['attributes']:
        if att['name'] == attributeName:
            return att
    return None

def addAttributeToItem(cursor: psycopg2.extensions.cursor, item, attribute, value: float):
    cursor.execute("insert into craft_items_attributes(itemid, attributeid, value) values (" + str(item['id']) + ", " + str(attribute['id']) + ", " + str(value) + ")")
    return cursor.rowcount == 1


amountItemsNotFoundInDb = 0
amountItemsNotFoundInWsDb = 0
amountItemsNotFoundInWsDbInARow = 0
attributesToBonus: list = readJson('./attributesToBonus.json')

inDevelopment = os.getenv('DEV') == "y"

def getBonusName(bonus: int):
    for x in attributesToBonus:
        if int(x['bonus']) == bonus:
            return str(x['name'])
    return None

try:
    cursor = conn.cursor()
    dbAttributes = getDbAttribtues(cursor)

    # get items parameters
    for item in items:
        craftItem = getDbItem(cursor, item)
        if craftItem == None:
            amountItemsNotFoundInDb += 1
            continue
        amountItemsNotFoundInWsDbInARow = 0
        if craftItem['level'] == None:
            continue
        
        if inDevelopment:
            if craftItem['id'] < 140 or craftItem['level'] % 2 != 0:
                continue
            if craftItem['id'] > 1000:
                break

        wsDbitemData = getItemData(int(item['id']))
        if wsDbitemData == None:
            amountItemsNotFoundInWsDb += 1
            amountItemsNotFoundInWsDbInARow += 1
            if amountItemsNotFoundInWsDbInARow > 5:
                print("Five items not found in ws db, check the internet connection and try later")
                break
            continue
        bonusAdded = 0
        for x in range(1,4):
            if wsDbitemData['bonus' + str(x)] == None:
                continue
            bonus = int(wsDbitemData['bonus' + str(x)])
            
            bonusName = getBonusName(bonus)
            if bonusName == None:
                bonusName = str(wsDbitemData['bonus' + str(x) + 'Name']) + ' - ' + str(bonus)
                attributesToBonus.append({'name': bonusName, 'bonus': bonus})
            
            att = findAttribute(dbAttributes, bonusName)
            if att == None:
                att = addAttribute(cursor, bonusName)
                print("Attribute added. Id: " + str(att['id']) + " - name: " + att['name'])
                dbAttributes.append(att)
            
            value = float(int(wsDbitemData['value' + str(x)]) / 100)

            if craftItemHasThisAttribute(craftItem, att['name']):
                continue
            
            if not addAttributeToItem(cursor, craftItem, att, value):
                print("Unable to add attribute " + att['name'] + "(" + str(att['id']) + ") with value " + str(value) + " to item " + craftItem['name'] + "(" + str(craftItem['id']) + ")")
            bonusAdded += 1
            conn.commit()
        
        for x in range(1,3):
            if wsDbitemData['setBonus' + str(x)] == None:
                continue
            bonus = int(wsDbitemData['setBonus' + str(x)])
            
            bonusName = getBonusName(bonus)
            if bonusName == None:
                bonusName = str(wsDbitemData['setBonus' + str(x) + 'Name']) + ' - ' + str(bonus)
                attributesToBonus.append({'name': bonusName, 'bonus': bonus})
            bonusName = "Set - " + bonusName
            att = findAttribute(dbAttributes, bonusName)
            if att == None:
                att = addAttribute(cursor, bonusName)
                print("Attribute added. Id: " + str(att['id']) + " - name: " + att['name'])
                dbAttributes.append(att)
            
            value = float(int(wsDbitemData['setValue' + str(x)]) / 100)

            if craftItemHasThisAttribute(craftItem, att['name']):
                continue
            
            if not addAttributeToItem(cursor, craftItem, att, value):
                print("Unable to add attribute " + att['name'] + "(" + str(att['id']) + ") with value " + str(value) + " to item " + craftItem['name'] + "(" + str(craftItem['id']) + ")")
            bonusAdded += 1
            conn.commit()
        if bonusAdded > 0:
            print(str(bonusAdded) + " bonuses added to item " + craftItem['name'] + "("+ str(craftItem['id']) + ")")
        if inDevelopment:
            time.sleep(1)
except Exception as err:
    print(err.with_traceback())
finally:
    conn.close()
    print("amount items not found in db: " + str(amountItemsNotFoundInDb))
    print("amount items not found in wsdb: " + str(amountItemsNotFoundInWsDb))
    with open('./attributesToBonus.json', '+w') as file:
        file.truncate()
        json.dump(attributesToBonus, file, indent=4)
