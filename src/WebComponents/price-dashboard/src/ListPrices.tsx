import React, { useCallback, useEffect, useState } from "react";
import * as api from './api';
import { IItemPrice, IStandardPaginationParams } from "./interfaces";
import Pagination from "./Pagination";
import UpdatePriceModal from "./UpdatePriceModal";

const updatePriceModalId = "updatePriceModal";

export interface IListPricesProps {
    manufacturer: string;
}

const ListPrices: React.FC<IListPricesProps> = ({
    manufacturer
}) => {
    const [pricesToEdit, setPricesToEdit] = useState<{
        [id: string]: IItemPrice
    }>({})

    const addOrRemovePriceToEdit = useCallback((price: IItemPrice) => {
        setPricesToEdit(old => {
            const newValues = {...old}
            if (Object.keys(newValues).includes(price.id))
                delete newValues[price.id]
            else
                newValues[price.id] = price
            return newValues
        })
    }, [setPricesToEdit])

    const [errorMessage, setErrorMessage] = useState<string>();
    const [searchingForPrices, setSearchingForPrices] = useState(false);
    const [data, setData] = useState<IItemPrice[]>([]);
    
    const [pagination, setPagination] = useState<IStandardPaginationParams>({count: 10, start: 0});
    const [totalItems, setTotal] = useState(0);

    const searchPrices = useCallback((p: IStandardPaginationParams) => {
        if (!manufacturer)
            return
        setSearchingForPrices(true)
        setPricesToEdit({})
        api.listPrices(manufacturer, p)
        .then(result => {
            setTotal(result.total)
            setData(result.data)
        })
        .catch(r => {
            setErrorMessage(`Error on list prices. Info: ${JSON.stringify(r)}`)
        })
        .finally(() => {
            setSearchingForPrices(false);
        });
    }, [manufacturer])

    useEffect(() => {
        searchPrices(pagination)
    }, [manufacturer]);
    
    return <section className="container text-center mt-1">
        {
            errorMessage !== undefined &&
            <p>{errorMessage}</p>
        }
        <div className="row justify-content-start mb-1">
            <div className="col-4 col-sm-1">
                <button 
                    className="btn btn-primary" 
                    type='button'
                    onClick={() => searchPrices(pagination)}
                >
                    Refresh
                </button>
            </div>
            <div className="col-1 col-sm-1">
                <UpdatePriceModal
                    id={updatePriceModalId}
                    manufacturer={manufacturer}
                />
                <button 
                    className="btn btn-secondary" 
                    type='button'
                    data-bs-toggle="modal" 
                    data-bs-target={"#" + updatePriceModalId}
                >
                    Edit
                </button>
            </div>
        </div>
        {
            searchingForPrices ?
            <p>Searching items prices...</p>
            :
            (
                data.length === 0 ?
                <p>No items found</p>
                :
                <table className="table">
                    <thead>
                        <tr>
                            <th> </th>
                            <th>Icon</th>
                            <th>Name</th>
                            <th>Level</th>
                            <th>Price</th>
                        </tr>
                    </thead>
                    <tbody>
                    {
                        data.map(v => (<tr key={v.id}>
                            <td>
                                <input 
                                    type="checkbox" 
                                    onClick={() => addOrRemovePriceToEdit(v)} 
                                    defaultChecked={pricesToEdit[v.id] !== undefined} 
                                />
                            </td>
                            <td><img src={import.meta.env.VITE_AssetsUrl + '/' + v.item.asset?.endpoint} /></td>
                            <td>{v.item.name}</td>
                            <td>{v.item.level}</td>
                            <td>{v.price}</td>
                        </tr>))
                    }
                    </tbody>
                </table>
            )
        }
        <div className="row justify-content-center">
            <div className="col-1">
                <Pagination 
                    {...pagination} 
                    total={totalItems} 
                    goTo={i => {
                        setPagination(p => ({...p, start: i}))
                        searchPrices({...pagination, start: i})
                    }} 
                />
            </div>
        </div>
    </section>
}

export default ListPrices;