import { IAddCraftResource, IApiError, IAsset, ICraftResource, IItem, IItemToAdd, IListAssetsParams, IListItemsParams, IStandardList, IStaticData, IUpdateCraftResource } from "./interfaces";
import { isApiError } from "./typeCheck";

const baseUrl = import.meta.env.VITE_ApiUrl;

let defaultHeaders = new Headers();
defaultHeaders.append("Content-Type", "application/json");
    if (import.meta.env.DEV)
        defaultHeaders.append(
            "Authorization", 
            `Bearer ${import.meta.env.VITE_Identifier}`
        );

async function handleError(resp: Response): Promise<IApiError> {
    const content = await resp.text();
    let data = JSON.parse(content);
    if (!isApiError(data))
        data = { message: `Unknowed error on request the server. Contact the developers. Status: ${resp.status}` } as IApiError;
    return data;
}

function getQueryString(values: any): string {
    const keys = Object.keys(values);
    let query = "";
    for (let index = 0; index < keys.length; index++) {
        if (values[keys[index]] !== undefined && values[keys[index]] !== null)
            query += keys[index] + '=' + values[keys[index]] + "&";
    }
    return query;
}

export async function listItems(p: IListItemsParams) {
    return fetch(
            `${baseUrl}/Items?${getQueryString(p)}`, 
            { headers: defaultHeaders }
        )
        .then(async r => {
            if (r.status === 200)
                return JSON.parse(await r.text()) as IStandardList<IItem>;
            return handleError(r);
        });
}

export async function listItemResources(itemId: number) {
    return fetch(
            `${baseUrl}/Items/${itemId}/resources`, 
            { headers: defaultHeaders }
        )
        .then(async r => {
            if (r.status === 200)
                return JSON.parse(await r.text()) as IStandardList<ICraftResource>;
            return handleError(r);
        });
}

export async function addItemResource(data: IAddCraftResource) {
    return fetch(
            `${baseUrl}/CraftResources`, 
            { 
                headers: defaultHeaders, 
                method: 'POST', 
                body: JSON.stringify(data) 
            }
        )
        .then(async r => {
            if (r.status === 201)
                return JSON.parse(await r.text()) as IStandardList<ICraftResource>;
            return handleError(r);
        });
}

export async function deleteItemResource(data: ICraftResource) {
    return fetch(
        `${baseUrl}/CraftResources/${data.id}`, 
        { 
            headers: defaultHeaders, 
            method: 'DELETE', 
            body: JSON.stringify(data) 
        }
    )
    .then(async r => {
        if (r.status === 204)
            return true;
        return handleError(r);
    });
}

export async function updateItemResource(
    id: ICraftResource['id'], 
    data: IUpdateCraftResource
) {
    return fetch(
        `${baseUrl}/CraftResources/${id}`, 
        { 
            headers: defaultHeaders, 
            method: 'PUT', 
            body: JSON.stringify(data) 
        }
    )
    .then(async r => {
        if (r.status === 204)
            return true;
        return handleError(r);
    });
}

export async function getStaticData() {
    return fetch(
        `${baseUrl}/Static/data`, 
        { headers: defaultHeaders }
    )
    .then(async r => {
        if (r.status === 200)
            return JSON.parse(await r.text()) as IStaticData;
        return handleError(r);
    });
}

export async function deleteItems(
    id: ICraftResource['id']
) {
    return fetch(
        `${baseUrl}/Items/${id}`, 
        { 
            headers: defaultHeaders, 
            method: 'DELETE'
        }
    )
    .then(async r => {
        if (r.status === 204)
            return true;
        return handleError(r);
    });
}

export async function addItem(item: IItemToAdd) {
    return fetch(
        `${baseUrl}/Items`, 
        { 
            headers: defaultHeaders, 
            method: 'POST',
            body: JSON.stringify(item)
        }
    )
    .then(async r => {
        if (r.status === 201)
            return JSON.parse(await r.text()) as IItem;
        return handleError(r);
    });
}

export async function listAssets(p: IListAssetsParams) {
    return fetch(
            `${baseUrl}/Assets?${getQueryString(p)}`, 
            { headers: defaultHeaders }
        )
        .then(async r => {
            if (r.status === 200)
                return JSON.parse(await r.text()) as IStandardList<IAsset>;
            return handleError(r);
        });
}

export async function getItem(itemId: number) {
    return fetch(
        `${baseUrl}/Items/${itemId}`, 
        { 
            headers: defaultHeaders, 
            method: 'GET'
        }
    )
    .then(async r => {
        if (r.status === 200)
            return JSON.parse(await r.text()) as IItem;
        return handleError(r);
    });
}