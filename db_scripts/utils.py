import json

def readJson(path):
    with open(path, 'r') as file:
        return json.load(file)