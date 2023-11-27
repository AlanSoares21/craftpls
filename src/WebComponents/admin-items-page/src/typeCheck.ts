import { IApiError } from "./interfaces";

export function isApiError(value: any): value is IApiError {
    return typeof value["message"] === "string" 
        && Object.keys(value).length === 1;
}