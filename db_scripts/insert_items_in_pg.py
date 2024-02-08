import json
from utils import itemNameToPostgresString, readJson

itemTypeToCraftCategory = readJson("./itemTypeToCraftCategory.json")
itemsTypes = readJson("./itemsTypes.json")

def readItemTypeFile(itemType: str):
    return readJson('./data/' + itemType + ".json")

repetedData = {}

def commandForFile(itemType: str):
    command = "-- inserting items of type " + itemType + " \ninsert into craft_items(name, level, categoryId) values "
    repeted = {}
    items = readItemTypeFile(itemType)["list"]
    ammountItems = len(items)
    firstAdded = False
    for index in range(ammountItems):
        item = items[index]
        level = int(item['level'])
        if level < 10 or level % 2 != 0:
            continue
        name = itemNameToPostgresString(str(item['name']))
        repetedKey = name.replace(" ", "") + '-' + str(level)
        if repetedKey in repeted:
            if repeted[repetedKey] > 0:
                repeted[repetedKey] += 1
                continue
        else:
            repeted[repetedKey] = 1
        if firstAdded:
            command += ",\n('" + name + "', " + str(item['level']) + ", " + str(itemTypeToCraftCategory[itemType]) + ")"
        else:
            command += "\n('" + name + "', " + str(item['level']) + ", " + str(itemTypeToCraftCategory[itemType]) + ")"
            firstAdded = True
    repetedData[itemType] = repeted
    return command + ";\n\n\n\n"

def createCommandString() -> str:
    command = ""
    for key in itemsTypes:
        command += commandForFile(key)
    return command

with open("./insertItemsScript.sql", 'w') as file:
    file.write(createCommandString())

with open("./items_repeted.json", 'w') as file:
    file.truncate()
    json.dump(repetedData, file, indent=4)