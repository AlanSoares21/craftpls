import React, { useCallback, useEffect, useState } from "react"
import Pagination from "./Pagination"
import { listItems } from "./api";
import { IFilterItems, IItem, IListItemsParams } from "./interfaces";

interface ISelectItemProps {
    itemSelected(item: IItem): void;
}

const SelectItem: React.FC<ISelectItemProps> = ({itemSelected}) => {
    const [items, setItems] = useState<IItem[]>([]);

    const [total, setTotal] = useState(1);
    const [pagination, setPagination] = useState<IListItemsParams>({
        count: 5,
        start: 0
    });
    const [filterValues, setFilter] = useState<IFilterItems>({});

    const searchItems = useCallback((p: IListItemsParams) => {
        listItems(p)
        .then(r => {
            if (r.total !== total)
                setTotal(r.total)
            setItems(r.data)
        });
    }, [total]);

    useEffect(() => {
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
                    onChange={ev => {
                        if (ev.target.value)
                            setFilter(p => ({...p, name: ev.target.value}))
                        else
                            setFilter(p => ({...p, name: undefined}))
                    }}
                />
            </div>
        </div>
        <div className="row mb-2">
            <div className="col">
                <label className="form-label" htmlFor="txtLevel">Level</label>
                <input 
                    id="txtLevel" 
                    type="number" 
                    className="form-control"
                    min={0} 
                    onChange={ev => {
                        let level = parseInt(ev.target.value)
                        if (Number.isInteger(level) && level > 0)
                            setFilter(p => ({...p, level}))
                        else
                            setFilter(p => ({...p, level: undefined}))
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
                    onChange={ev => {
                        let maxLevel = parseInt(ev.target.value)
                        if (Number.isInteger(maxLevel) && maxLevel > 0)
                            setFilter(p => ({...p, maxLevel}))
                        else
                            setFilter(p => ({...p, maxLevel: undefined}))
                    }}
                />
            </div>
            <div className="col">
                <label className="form-label" htmlFor="txtMinLevel">Min Level</label>
                <input 
                    id="txtMinLevel"
                    type="number" 
                    className="form-control" 
                    min={0} 
                    onChange={ev => {
                        let minLevel = parseInt(ev.target.value)
                        if (Number.isInteger(minLevel) && minLevel > 0)
                            setFilter(p => ({...p, minLevel}))
                        else
                            setFilter(p => ({...p, minLevel: undefined}))
                    }}
                />
            </div>
        </div>
        <div className="row mb-1">
            <button 
                className="btn btn-primary"
                onClick={() => {
                    searchItems({...pagination, ...filterValues})
                    setPagination({...pagination, ...filterValues})
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
                                <tr key={item.id} onClick={() => itemSelected(item)}>
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
    itemSelected(item?: IItem): void;
}

const ItemSelected: React.FC<IItemSelectedProps> = ({itemSelected}) => {
    const [item, setItem] = useState<IItem>();

    return <div>
        {
            item === undefined &&
            <SelectItem itemSelected={i => {
                setItem(i)
                itemSelected(i)
            }} />
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
            </>
        }
    </div>
};

export default ItemSelected