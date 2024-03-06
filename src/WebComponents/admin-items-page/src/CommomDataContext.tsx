import React, { PropsWithChildren, useCallback, useEffect, useState } from "react";
import { IItem, IStaticData } from "./interfaces";
import { getStaticData } from "./api";
import { isApiError } from "./typeCheck";

export interface ICommomDataContext {
    static: IStaticData
    lastResourcesAdded: IItem[]
    addResourceInCache(item: IItem, amount: number): any
    getResourceAmountInCache(id: IItem['id']): number
}

export const CommomDataContext = React.createContext<ICommomDataContext>({
    addResourceInCache() {},
    getResourceAmountInCache: () => 1,
    lastResourcesAdded: [],
    static: {
        categories: []
    }
});

export const CommomDataProvider: React.FC<PropsWithChildren> = ({children}) => {
    const [staticData, setStaticData] = useState<ICommomDataContext['static']>({categories: []})
    const [lastResourcesAdded, setLastResourcesAdded] = useState<IItem[]>([])
    const [resourcesAmount, setResourcesAmount] = 
        useState<{id: IItem['id'], amount: number}[]>([])

    const getResourceAmountInCache = useCallback<ICommomDataContext['getResourceAmountInCache']>(id => {
        const index = resourcesAmount.findIndex(r => r.id == id)
        if (index === -1 || resourcesAmount[index] === undefined)
            return 1
        return resourcesAmount[index].amount
    }, [resourcesAmount]);

    const addResourceInCache = useCallback((item: IItem, amount: number) => {
        let list = [...lastResourcesAdded]
        let listAmount = [...resourcesAmount]
        const index = list.findIndex(r => r.id == item.id)
        if (index != -1) {
            list.splice(index, 1)
            listAmount.splice(index, 1)
        } else if (list.length >= 5) {
            list.splice(0, 1)
            listAmount.splice(0, 1)
        }
        list.push(item)
        listAmount.push({id: item.id, amount})
        setLastResourcesAdded(list)
        setResourcesAmount(listAmount)
    }, [lastResourcesAdded, resourcesAmount]);
    
    useEffect(() => {
        getStaticData().then(r => {
            if (isApiError(r)) {
                alert("Error on request static data. Message: " + r.message)
            } else {
                setStaticData( v=> ({...v, static: r}))
            }
        });
    }, [])

    return <CommomDataContext.Provider value={{
        static: staticData,
        addResourceInCache,
        lastResourcesAdded,
        getResourceAmountInCache
    }}>
        {children}
    </CommomDataContext.Provider>
}