import React, { PropsWithChildren, useEffect, useState } from "react";
import { IListItemsParams, IStaticData } from "./interfaces";
import { getStaticData } from "./api";
import { isApiError } from "./typeCheck";

export interface ICommomDataContext {
    static: IStaticData
    newPricesListItemsParams: IListItemsParams
    setNewPricesListItemsParams(filter: IListItemsParams): any
}

const defaultData: ICommomDataContext = {
    static: {
        categories: []
    },
    newPricesListItemsParams: {start: 0, count: 5, onlyListItemsWithResources: true},
    setNewPricesListItemsParams: () => {}
}

export const CommomDataContext = React.createContext<ICommomDataContext>(defaultData);

export const CommomDataProvider: React.FC<PropsWithChildren> = ({children}) => {
    const [staticData, setStaticData] = useState(defaultData.static)
    const [newPricesListItemsParams, setNewPricesListItemsParams] = 
        useState(defaultData.newPricesListItemsParams)
    
    useEffect(() => {
        getStaticData().then(r => {
            if (isApiError(r))
                alert("Error on request static data. Message: " + r.message)
            else
                setStaticData(r)
        })
    }, [])

    return <CommomDataContext.Provider 
        value={{
            static: staticData,
            newPricesListItemsParams,
            setNewPricesListItemsParams
        }}
    >
        {children}
    </CommomDataContext.Provider>
}