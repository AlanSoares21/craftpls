
export interface IStandardPaginationParams {
    start: number;
    count: number;
}

export interface IFilterItems {
    name?: string;
    level?: number;
    maxLevel?: number;
    minLevel?: number;
}

export interface IListItemsParams extends IStandardPaginationParams, IFilterItems {
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
    assetId?: number
    // prices: any[]
    resources: ICraftResource[]
    resourceFor: ICraftResource[]
    // requests: any[]
    asset?: IAsset
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