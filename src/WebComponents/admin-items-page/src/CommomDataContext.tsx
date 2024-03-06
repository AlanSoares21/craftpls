import React, { PropsWithChildren, useCallback, useEffect, useState } from "react";
import { IItem, IStaticData } from "./interfaces";
import { getStaticData } from "./api";
import { isApiError } from "./typeCheck";

export interface ICommomDataContext {
    static: IStaticData
    lastResourcesAdded: IItem[]
    addResourceInCache(item: IItem): any
}

const defaultData: ICommomDataContext = {
    static: {
        categories: []
    },
    lastResourcesAdded: [],
    addResourceInCache: () => {
        
    }
}

export const CommomDataContext = React.createContext<ICommomDataContext>(defaultData);

export const CommomDataProvider: React.FC<PropsWithChildren> = ({children}) => {
    const [value, setValue] = useState(defaultData)
    const [lastResourcesAdded, setLastResourcesAdded] = useState<IItem[]>([])

    const addResourceInCache = useCallback((item: IItem) => {
        let list = [...lastResourcesAdded]
        const index = list.findIndex(r => r.id == item.id)
        if (index != -1) {
            list.splice(index, 1)
        } else if (list.length >= 5) {
            list.splice(0, 1)
        }
        list.push(item)
        setLastResourcesAdded(list);
    }, [lastResourcesAdded]);
    
    useEffect(() => {
        getStaticData().then(r => {
            if (isApiError(r)) {
                alert("Error on request static data. Message: " + r.message)
            } else {
                setValue( v=> ({...v, static: r}))
            }
        });
    }, [])

    return <CommomDataContext.Provider value={{
        static: value.static,
        addResourceInCache,
        lastResourcesAdded
    }}>
        {children}
    </CommomDataContext.Provider>
}