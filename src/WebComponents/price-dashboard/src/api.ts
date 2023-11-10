import axios from 'axios'
import { 
    IAddItemPrice, IItem, IItemPrice, 
    IListItemsParams, 
    IStandardList, IStandardPaginationParams, 
    IUpdateItemPrice 
} from './interfaces';

const url = import.meta.env.VITE_ApiUrl;

const authHeaders = {
    "Authorization": `Bearer ${import.meta.env.VITE_Identifier}`
};

const api = axios.create();

export function listItems(params: IListItemsParams) {
    return api.get<IStandardList<IItem>>(
        `${url}/Items`, 
        {params, headers: authHeaders})
        .then(r => r.data);
}

export function listPrices(manufacturerId: string, params: IStandardPaginationParams) {
    return api.get<IStandardList<IItemPrice>>(
            `${url}/Prices/${manufacturerId}`, 
            {params, headers: authHeaders}
        )
        .then(r => r.data);
}

export function addPrice(data: IAddItemPrice) {
    return api.post<IAddItemPrice>(
        `${url}/Prices`, data,
        {headers: authHeaders})
        .then(r => r.data);
}

export function updatePrice(priceId: string, data: IUpdateItemPrice) {
    return api.put<IAddItemPrice>(
        `${url}/Prices/${priceId}`,
        data,
        {headers: authHeaders})
        .then(r => r.data);
}
