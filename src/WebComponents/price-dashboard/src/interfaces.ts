
export interface IStandardPaginationParams {
    start: number;
    count: number;
}

export interface IListPriceParams extends IStandardPaginationParams {
    itemId?: number
    resourcesOf?: number
}

export interface IStandardList<T> {
    start: number;
    count: number;
    total: number;
    data: T[];
}

export interface IFilterItems {
    name?: string;
    level?: number;
    maxLevel?: number;
    minLevel?: number;
}

export interface IListItemsParams extends IStandardPaginationParams, IFilterItems {
}

export interface IAsset {
    endpoint: string;
}

export interface IItem {
    id: number;
    name: string;
    level: number;
    asset: IAsset | null;
}

export interface ICraftResource {
    id: number
    amount: number
    resourceId: number
    resource: IItem
    itemId: number
    item: IItem
  }

export interface IItemPrice {
    id: string
    price: number
    totalPrice: number
    itemId: IItem['id']
    item: IItem
    manufacturerId: string
    resourcesChanged: boolean
}

export interface IAddItemPrice {
    price: number;
    itemId: IItem['id'];
    manufacturerId: string;
}

export interface IUpdateItemPrice {
    price: number;
}

export interface IApiError {
    message: string
}