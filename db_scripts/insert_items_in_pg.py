from utils import readJson

itemTypeToCraftCategory = readJson("./itemTypeToCraftCategory.json")
itemsTypes = readJson("./itemsTypes.json")

def readItemTypeFile(itemType: str):
    return readJson('./data/' + itemType + ".json")

def commandForFile(itemType: str):
    command = "-- inserting items of type " + itemType + " \ninsert into craft_items(name, level, categoryId) values "
    items = readItemTypeFile(itemType)["list"]
    ammountItems = len(items)
    lastIndex = ammountItems - 1
    for index in range(ammountItems):
        item = items[index]
        name = str(item['name']).replace('\'', '\'\'')
        command += "('" + name + "', " + str(item['level']) + ", " + str(itemTypeToCraftCategory[itemType]) + ")"
        if (index != lastIndex):
            command += ",\n"
    return command + ";\n\n\n\n"

def createCommandString() -> str:
    command = ""
    for key in itemsTypes:
        command += commandForFile(key)
    return command

with open("./insertItemsScript.sql", 'w') as file:
    file.write(createCommandString())