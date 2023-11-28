import { IAsset } from "./interfaces";

export function getAssetUrl(asset: IAsset) {
    return import.meta.env.VITE_AssetsUrl 
    + '/' 
    + asset.endpoint;
}