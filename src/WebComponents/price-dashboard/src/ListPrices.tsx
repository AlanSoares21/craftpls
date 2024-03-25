import React, { useCallback, useContext, useEffect, useState } from "react";
import * as api from './api';
import { IItemPrice, IListPriceParams } from "./interfaces";
import Pagination from "./Pagination";
import UpdatePriceModal from "./UpdatePriceModal";
import CheckResourcesModal from "./CheckResourcesModal";
import { Badge } from "react-bootstrap";
import { CommomDataContext } from "./CommomDataContext";

const updatePriceModalId = "updatePriceModal";

export interface IListPricesProps {
    manufacturer: string;
}

const ListPrices: React.FC<IListPricesProps> = ({
    manufacturer
}) => {
    const commomData = useContext(CommomDataContext)

    const [pricesToEdit, setPricesToEdit] = useState<{
        [id: string]: IItemPrice
    }>({})

    const [pricesToCheckResource, setPriceToCheckResource] = useState<IItemPrice>()

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
    
    const [pagination, setPagination] = useState<IListPriceParams>({count: 10, start: 0});
    const [totalItems, setTotal] = useState(0);

    const searchPrices = useCallback((p: IListPriceParams) => {
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
        <div className="row mb-1">
            <div className="col-auto">
                <label htmlFor="slcCategoryForList">Category</label>
                <select 
                    id="slcCategoryForList" 
                    className="form-select"
                    onChange={ev => {
                        const id = parseInt(ev.currentTarget.value)
                        if (id > 0)
                            setPagination(p => ({...p, itemCategory: id}))
                        else
                            setPagination(p => ({...p, itemCategory: undefined}))
                    }}
                >
                    <option value={-1}>Any</option>
                    {
                        commomData.static.categories.map(c => (
                            <option key={c.id} value={c.id}>{c.name}</option>
                        ))
                    }
                </select>
            </div>
            <div className="col-md-6 col-sm-auto">
                <label htmlFor="txtNameForList">Name</label>
                <input 
                    id="txtNameForList" 
                    className="form-control" 
                    type="text" 
                    onChange={ev => {
                        const value = ev.currentTarget.value.trim()
                        if (value)
                            setPagination(p => ({...p, itemName: value}))
                        else
                            setPagination(p => ({...p, itemName: undefined}))
                    }}
                />
            </div>
            <div className="col-auto pt-4">
                <button 
                    className="btn btn-primary" 
                    type='button'
                    onClick={() => {
                        const value = {...pagination, start: 0}
                        setPagination(value)
                        searchPrices(value)
                    }}
                >
                    Refresh
                </button>
            </div>
            <div className="col-auto pt-4">
                <button 
                    className="btn btn-secondary" 
                    type='button'
                    data-bs-toggle="modal" 
                    data-bs-target={"#" + updatePriceModalId}
                >
                    Edit Selected Items
                </button>
            </div>
        </div>
        <div style={{width: '100%', overflow: 'scroll'}}>
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
                                <th>Total Price</th>
                                <th>Craft Price</th>
                                <th>Status</th>
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
                                <td>{v.totalPrice}</td>
                                <td>{v.price}</td>
                                <td>
                                    {
                                        v.resourcesChanged && 
                                        <Badge 
                                            bg='warning'
                                            onClick={() => setPriceToCheckResource(v)}
                                        >
                                            Click to check resources
                                        </Badge>
                                    }
                                </td>
                            </tr>))
                        }
                        </tbody>
                    </table>
                )
            }
        </div>
        <div className="row justify-content-center">
            <div className="col">
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
        <CheckResourcesModal 
            open={pricesToCheckResource !== undefined}
            onClose={() => setPriceToCheckResource(undefined)}
            onSuccess={() => setPriceToCheckResource(undefined)}
            changePrice={setPriceToCheckResource}
            manufacturer={manufacturer}
            price={pricesToCheckResource}
        />
        <UpdatePriceModal
            id={updatePriceModalId}
            manufacturer={manufacturer}
            itemsPrices={pricesToEdit}
            onUpdate={() => {
                searchPrices(pagination)
            }}
        />
    </section>
}

export default ListPrices;