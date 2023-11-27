/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_ApiUrl: string;
    readonly VITE_AssetsUrl: string;

    // only on dev
    readonly VITE_Identifier?: string;
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}