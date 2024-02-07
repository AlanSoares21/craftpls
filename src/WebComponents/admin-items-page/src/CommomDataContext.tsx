import React, { PropsWithChildren, useEffect, useState } from "react";
import { IStaticData } from "./interfaces";
import { getStaticData } from "./api";
import { isApiError } from "./typeCheck";

export interface ICommomDataContext {
    static: IStaticData
}

const defaultData: ICommomDataContext = {
    static: {
        categories: []
    }
}

export const CommomDataContext = React.createContext<ICommomDataContext>(defaultData);

export const CommomDataProvider: React.FC<PropsWithChildren> = ({children}) => {
    const [value, setValue] = useState(defaultData)
    
    useEffect(() => {
        getStaticData().then(r => {
            if (isApiError(r))
                alert("Error on request static data. Message: " + r.message)
            else
                setValue({static: r})
        })
    }, [])

    return <CommomDataContext.Provider {...{value}}>
        {children}
    </CommomDataContext.Provider>
}