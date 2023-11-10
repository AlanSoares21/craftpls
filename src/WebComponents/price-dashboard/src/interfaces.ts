
export interface IStandardPaginationParams {
    start: number;
    count: number;
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
    asset?: IAsset;
}

export interface IItemPrice {
    id: string;
    price: number;
    itemId: IItem['id'];
    item: IItem;
    manufacturerId: string;
}

export interface IAddItemPrice {
    price: number;
    itemId: IItem['id'];
    manufacturerId: string;
}

export interface IUpdateItemPrice {
    price: number;
}
