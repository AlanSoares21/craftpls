import requests as rt
from utils import readJson
import json

itemTypeToCraftCategory = readJson("./itemTypeToCraftCategory.json")
itemsTypes = readJson("./itemsTypes.json")

baseUrl = "https://wsdb.xyz/api/data"

def listItems(itemType: str):
    url = baseUrl + "/item/items/pt/" + str(itemsTypes[itemType])
    print("searching for " + itemType + " in " + url)
    response = rt.get(url)
    print("searching for " + itemType + " - status: " + str(response.status_code))
    return response.json()

for key in itemsTypes:
    items = listItems(key)
    itemsJson = json.dumps(items, indent=4)
    with open("./data/pt/" + key + ".json", "w") as file:
        file.write(itemsJson)