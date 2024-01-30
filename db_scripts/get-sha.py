import hashlib
import json
import os

folderPath = './get-sha'
map = {}

def getImageSha256(name: str, data):
    sha = hashlib.sha256(data).hexdigest(); 
    print("Sha for " + name + " is " + sha)
    return sha

imagesFilename = [f for f in os.listdir(folderPath) if f.endswith('.webp')]

print(imagesFilename)

insertCommand = "insert into assets(endpoint) values"

for filename in imagesFilename:
    with open(folderPath + '/' + filename, 'rb') as file:
        sha = getImageSha256(filename, file.read())
        map[filename] = sha
    try:
        os.rename(folderPath + '/' + filename, folderPath + '/' + map[filename] + '.webp')
        if insertCommand[- 1] == '\n':
            insertCommand += ", ('" + map[filename] + ".webp') \n"
        else:
            insertCommand += " ('" + map[filename] + ".webp') \n"
    except Exception as ex:
        print("Error rename file " + filename)
        print(ex.with_traceback())

mapFilename = 'map.json'

# removing old mapping data
if len([f for f in os.listdir(folderPath) if f == mapFilename]) > 0:
    os.remove(folderPath + '/' + mapFilename)
# saving mapping data
with open(folderPath + '/' + mapFilename, 'w') as mapFile:
    mapFile.write(json.dumps(map, indent=4))

insertFilename = 'insertAssets.sql'
with open(folderPath + '/' + insertFilename, 'w') as insertAssetsFile:
    insertAssetsFile.write(insertCommand)
