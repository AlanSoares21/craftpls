
export interface IStandardPaginationParams {
    start: number;
    count: number;
}

export interface IFilterItems {
    name?: string;
    level?: number;
    maxLevel?: number;
    minLevel?: number;
    categoryId?: number;
}

export interface IListItemsParams extends IStandardPaginationParams, IFilterItems {
}

export interface IListAssetsParams extends IStandardPaginationParams {
    unusedAssets?: boolean
    itemName?: string
    endpoint?: string
}

export interface IStandardList<TData> {
    count: number
    start: number
    total: number
    data: TData[]
}

export interface IApiError {
    message: string
}

export interface IItem {
    id: number
    name: string
    level: number
    categoryId?: number
    category?: any
    assetId: number | null
    // prices: any[]
    resources: ICraftResource[]
    resourceFor: ICraftResource[]
    // requests: any[]
    asset: IAsset | null
    attributes: IItemAttribute[]
}

export interface IItemAttribute {
    id: number
    value: number
    attribute: IAttribute
}

export interface IItemToAdd {
    name: string
    categoryId?: number
    level?: number
    assetId?: number
    attributes: {
        attributeId: number
        value: number
    }[]
    namesByCulture: {[culture: string]: string}
}

export interface ICraftResource {
    id: number
    amount: number
    resourceId: number
    resource: IItem
    itemId: number
    item: IItem
}

export interface IUpdateCraftResource { 
    amount: number
}

export interface IAsset {
    id: number
    endpoint: string
    craftItems: IItem[]
}
  
export interface IAddCraftResource {
    itemId: IItem['id']
    resourceId: IItem['id']
    amount: number
}

export interface IAttribute {
    id: number
    name: string
}

export interface IStaticData {
    categories: {
        id: number
        name: string
    }[]
    attributes: IAttribute[]
}