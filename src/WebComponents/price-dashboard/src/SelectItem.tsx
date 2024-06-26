import React, { useCallback, useContext, useEffect, useState } from "react"
import Pagination from "./Pagination"
import { listItemResources, listItems, listPrices } from "./api";
import { ICraftResource, IItem, IItemPrice, IListItemsParams } from "./interfaces";
import { Image, Stack, Table } from "react-bootstrap";
import { getAssetUrl } from "./utils";
import { CommomDataContext } from "./CommomDataContext";

interface ISelectItemProps {
    itemSelected(item: IItem): void;
}

const SelectItem: React.FC<ISelectItemProps> = ({itemSelected}) => {
    const commomData = useContext(CommomDataContext);

    const [items, setItems] = useState<IItem[]>([]);

    const [total, setTotal] = useState(1);
    const [pagination, setPagination] = useState<IListItemsParams>(commomData.newPricesListItemsParams);

    const searchItems = useCallback((p: IListItemsParams) => {
        listItems(p)
        .then(r => {
            if (r.total !== total)
                setTotal(r.total)
            setItems(r.data)
        });
    }, [total]);

    useEffect(() => {
        console.log({commomData, pagination})
        searchItems(pagination);
    }, []);

    return <div>
        <div className="row mb-1">
            <h6>Search item</h6>
        </div>
        <div className="row mb-1">
            <div className="col">
                <label className="form-label" htmlFor="txtName">Name</label>
                <input 
                    type="text" 
                    className="form-control"
                    defaultValue={pagination.name}
                    onChange={ev => {
                        if (ev.target.value)
                            setPagination(p => ({...p, name: ev.target.value}))
                        else
                            setPagination(p => ({...p, name: undefined}))
                    }}
                />
            </div>
        </div>
        <div className="row mb-1">
            <div className="col">
                <label className="form-label" htmlFor="txtLevel">Level</label>
                <input 
                    id="txtLevel" 
                    type="number" 
                    className="form-control"
                    min={0} 
                    defaultValue={pagination.level}
                    onChange={ev => {
                        let level = parseInt(ev.target.value)
                        if (Number.isInteger(level) && level > 0)
                            setPagination(p => ({...p, level}))
                        else
                            setPagination(p => ({...p, level: undefined}))
                    }}
                />
            </div>
            <div className="col">
                <label className="form-label" htmlFor="txtMaxLevel">Max Level</label>
                <input 
                    id="txtMaxLevel"
                    type="number" 
                    className="form-control" 
                    min={0}
                    defaultValue={pagination.maxLevel}
                    onChange={ev => {
                        let maxLevel = parseInt(ev.target.value)
                        if (Number.isInteger(maxLevel) && maxLevel > 0)
                            setPagination(p => ({...p, maxLevel}))
                        else
                            setPagination(p => ({...p, maxLevel: undefined}))
                    }}
                />
            </div>
            <div className="col">
                <label className="form-label" htmlFor="txtMinLevel">Min Level</label>
                <input 
                    id="txtMinLevel"
                    type="number" 
                    className="form-control" 
                    defaultValue={pagination.minLevel}
                    min={0} 
                    onChange={ev => {
                        let minLevel = parseInt(ev.target.value)
                        if (Number.isInteger(minLevel) && minLevel > 0)
                            setPagination(p => ({...p, minLevel}))
                        else
                            setPagination(p => ({...p, minLevel: undefined}))
                    }}
                />
            </div>
            <div className="col-12 mt-1">
                <input 
                    id="chkOnlyListItemWithResources"
                    type="checkbox" 
                    className="form-check-input" 
                    checked={pagination.onlyListItemsWithResources}
                    onChange={() => {
                        setPagination(p => ({...p, onlyListItemsWithResources: !p.onlyListItemsWithResources}))
                    }}
                />
                <label 
                    className="form-label" 
                    htmlFor="chkOnlyListItemWithResources"
                >
                    only list items with resources
                </label>
            </div>
        </div>
        <div className="row mb-2">
            <div className="col">
                <label className="form-label" htmlFor="selectCategory">Category</label>
                <select 
                    id="selectCategory"
                    className="form-select" 
                    onChange={ev => {
                        const id = parseInt(ev.currentTarget.value)
                        if (id > 0)
                            setPagination(p => ({...p, categoryId: id}))
                        else
                            setPagination(p => ({...p, categoryId: undefined}))
                    }}
                    defaultValue={pagination.categoryId}
                >
                    <option value={-1}>Any</option>
                    {
                        commomData.static.categories.map(c => (
                            <option key={c.id} value={c.id}>{c.name}</option>
                        ))
                    }
                </select>
            </div>
        </div>
        <div className="row mb-1">
            <button 
                className="btn btn-primary"
                onClick={() => {
                    searchItems({...pagination})
                    setPagination({...pagination})
                }}
            >
                Apply filter
            </button>
        </div>
        <div className="row mb-1">
            {
                items.length > 0 ?
                <table className="table table-hover">
                    <thead>
                        <tr>
                            <th scope="col"> </th>
                            <th scope="col">Name</th>
                            <th scope="col">Level</th>
                        </tr>
                    </thead>
                    <tbody>
                        {
                            items.map(item => (
                                <tr 
                                    key={item.id} 
                                    onClick={() => {
                                        console.log("set new filter")
                                        commomData.setNewPricesListItemsParams(pagination)
                                        itemSelected(item)
                                    }}
                                >
                                    <td><img src={import.meta.env.VITE_AssetsUrl + '/' + item.asset?.endpoint} /></td>
                                    <td>{item.name}</td>
                                    <td>{item.level}</td>
                                </tr>
                            ))
                        }
                    </tbody>
                </table>
                :
                <p>No items to show...</p>
            }
        </div>
        <div className="row">
            <Pagination 
                count={pagination.count} 
                start={pagination.start}
                total={total} 
                goTo={i => {
                    setPagination(p => ({...p, start: i}))
                    searchItems({...pagination, start: i})
                }} 
            />
        </div>
    </div>
}

export interface IItemSelectedProps {
    itemSelected(item?: IItem): void
    manufacturer: string
}

const ItemSelected: React.FC<IItemSelectedProps> = ({
    itemSelected, manufacturer
}) => {
    const [item, setItem] = useState<IItem>();
    const [missingResources, setMissingResources] = 
        useState<ICraftResource[]>([]);
    const [itemResourcesPrices, setItemResourcesPrices] = 
        useState<IItemPrice[]>([]);
    const [resources, setResources] = 
        useState<ICraftResource[]>([]);

    const checkResources = useCallback(async (itemToCheck: IItem) => {
        const listResources = listItemResources(itemToCheck.id);
        const resourcesPrices = await listPrices(manufacturer, {
            start: 0, 
            count: 10,
            resourcesOf: itemToCheck.id
        });
        const itemResources = await listResources.then(r => r.data);
        const missing = itemResources
            .filter(r => !resourcesPrices.data
                .some(p => p.itemId === r.resourceId)
            );
        setMissingResources(missing);
        setResources(itemResources);
        setItemResourcesPrices(resourcesPrices.data);
    }, [manufacturer, setMissingResources, setResources, setItemResourcesPrices]);

    const handleSelectItem = useCallback((item: IItem) => {
        checkResources(item)
        setItem(item)
        itemSelected(item)
    }, [checkResources, setItem, itemSelected]);

    return <div>
        {
            item === undefined &&
            <SelectItem itemSelected={handleSelectItem} />
        }
        {
            item !== undefined &&
            <>
                <div className="row mb-2">
                    <div className="col-1">
                        <img src={import.meta.env.VITE_AssetsUrl + '/' + item.asset?.endpoint} />
                    </div>
                    <div className="col-sm">
                        {item.name} - Lv: {item.level}
                    </div>
                </div>
                <div className="row">
                    <button 
                        className="btn btn-secondary" 
                        onClick={() => {
                            itemSelected(undefined)
                            setItem(undefined)
                        }}
                    >
                        Change Item
                    </button>
                </div>
                {
                    missingResources.length > 0 ?
                    <Stack>
                        <div>You should add prices to {missingResources.length} items before add a price to this item</div>
                        <Table>
                            <thead>
                                <tr>
                                    <th colSpan={4}>The following resources are missing a price</th>    
                                </tr>
                                <tr>
                                    <th>Icon</th>
                                    <th>Name</th>
                                    <th>Level</th>
                                    <th>Amount</th>
                                </tr>
                            </thead>
                            <tbody>
                                {missingResources.map(r => (
                                    <tr key={r.id} onClick={() => handleSelectItem(r.resource)}>
                                        <td>{r.resource.asset && <Image src={getAssetUrl(r.resource.asset)} />}</td>
                                        <td>{r.resource.name}</td>
                                        <td>{r.resource.level}</td>
                                        <td>{r.amount}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </Table>
                    </Stack>
                    :
                    <Table>
                        <thead>
                            <tr>
                                <th colSpan={5}>This item has the following resources</th>
                            </tr>
                            <tr>
                                <th>Icon</th>
                                <th>Name</th>
                                <th>Level</th>
                                <th>Total Price</th>
                                <th>Amount</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemResourcesPrices.map(p => (
                                <tr key={p.id}>
                                    <td>{p.item.asset && <Image src={getAssetUrl(p.item.asset)} />}</td>
                                    <td>{p.item.name}</td>
                                    <td>{p.item.level}</td>
                                    <td>{p.totalPrice}</td>
                                    <td>{resources.find(r => r.resourceId == p.itemId)?.amount}</td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                }
            </>
        }
    </div>
};

export default ItemSelected