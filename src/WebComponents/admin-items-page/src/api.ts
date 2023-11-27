import { IAddCraftResource, IApiError, ICraftResource, IItem, IListItemsParams, IStandardList } from "./interfaces";
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