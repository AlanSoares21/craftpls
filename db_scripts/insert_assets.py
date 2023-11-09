import requests as rt

from utils import readJson

baseUrl = 'https://wsdb.xyz/icons/'
iconsDirPath = 'data/icons'
iconsMapFilePath = iconsDirPath + '/iconsMap.json' 

iconsMap = readJson(iconsMapFilePath)

hashesUsed = []

with open('assetsToItemIcons.sql', 'w') as file:
    c = "insert into assets(endpoint) values \n"
    ammountIcons = len(iconsMap)
    i = 0
    for iconId in iconsMap:
        sha = str(iconsMap[iconId])
        if sha in hashesUsed:
            continue
        hashesUsed.append(sha)
        c += "('" + iconsMap[iconId] +".webp')"
        i += 1
        if (i != ammountIcons):
            c += ",\n"
    file.write(c)

