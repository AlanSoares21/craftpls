import { IAsset } from "./interfaces";

export function getAssetUrl(asset: IAsset) {
    return import.meta.env.VITE_AssetsUrl 
    + '/' 
    + asset.endpoint;
}

export function handleNumericInput(setter: (value: number | undefined) => any) {
    return (ev: React.ChangeEvent<HTMLInputElement>) => {
        if (ev.target.value) {
            const value  = parseInt(ev.target.value)
            setter(value);
        } else
            setter(undefined);
    }
}