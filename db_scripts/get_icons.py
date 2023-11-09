import hashlib
import json
import requests as rt
import os

from utils import readJson

baseUrl = 'https://wsdb.xyz/icons/'
iconsCachePath = 'data/icons_cache'
iconsDirPath = 'data/icons'
iconsMapFilePath = iconsDirPath + '/iconsMap.json' 

iconsCache = []

# fill icons cache
for file in os.listdir(iconsCachePath):
    if file.endswith('.webp'):
        iconsCache.append(int(file[:-5]))

iconsMap = readJson(iconsMapFilePath)

def getIcon(icon: int):
    iconName = str(icon) + ".webp"
    if not icon in iconsCache:
        print('Getting icon ' + iconName)
        content = rt.get(baseUrl + "/" + iconName).content
        with open(iconsCachePath + "/" + iconName, 'wb') as img:
            print('Writing ' + iconName + ' in the cache')
            img.write(content)
            print(iconName + ' is in the cache')
            iconsCache.append(icon)
            print(str(len(iconsCache)) + ' icons in the cache')
    with open(iconsCachePath + "/" + iconName, 'rb') as img:
        return img.read()

def getSha256(iconId: int, data):
    if not iconId in iconsMap:
        print("Creating sha for " + str(iconId))
        sha = hashlib.sha256(data).hexdigest(); 
        print("Sha for " + str(iconId) + " is " + sha)
        iconsMap[iconId] = sha
        print(str(len(iconsMap)) + " sha's registered")
    return iconsMap[iconId]

def saveIcon(name, data):
    with open(iconsDirPath + '/' + name, 'wb') as img:
        img.write(data)
        
def getIcons(data):
    for item in data['list']:
        iconId = int(item['icon'])
        print("Getting " + str(item['name']) + " lv " + str(item['level']) + " icon " + str(iconId))
        icon = getIcon(iconId)
        sha = getSha256(iconId, icon)
        print("Saving icon " + str(iconId) + " with sha256 " + sha)
        saveIcon(sha + ".webp", icon)

try:
    print("Icons in cache: " + str(len(iconsCache)))
    print("Sha's mapped: " + str(len(iconsMap)))
    for file in os.listdir('data'):
        if file.endswith('.json'):
            data = readJson("data/"+file)
            print("--- Getting icons for " + file)
            getIcons(data)
            print("--- finished get icons for " + file)
except Exception as ex:
    print("Error on get icons")
    print(ex.with_traceback())

print("saving icons map in " + iconsMapFilePath)
# save icons map
with open(iconsMapFilePath, 'w') as file:
    file.write(json.dumps(iconsMap, indent=4))