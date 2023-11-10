import React, { useCallback, useEffect, useState } from "react"
import Pagination from "./Pagination"
import { listItems } from "./api";
import { IItem, IListItemsParams } from "./interfaces";

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

    const searchItems = useCallback(() => {
        listItems(pagination)
        .then(r => {
            setTotal(r.total)
            setPagination(p => ({...p, start: r.start}))
            setItems(r.data)
        });
    }, []);

    useEffect(() => {
        searchItems();
    }, []);

    return <div>
        <div className="row mb-1">
            <h6>Search item</h6>
        </div>
        <div className="row mb-1">
            <div className="col">
                <label className="form-label" htmlFor="txtName">Name</label>
                <input type="text" className="form-control"/>
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
                />
            </div>
            <div className="col">
                <label className="form-label" htmlFor="txtMaxLevel">Max Level</label>
                <input 
                    id="txtMaxLevel"
                    type="number" 
                    className="form-control" 
                    min={0}
                />
            </div>
            <div className="col">
                <label className="form-label" htmlFor="txtMinLevel">Min Level</label>
                <input 
                    id="txtMinLevel"
                    type="number" 
                    className="form-control" 
                    min={0} 
                />
            </div>
        </div>
        <div className="row mb-1">
            <button className="btn btn-primary" onClick={searchItems}>
                Search Items
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
            <Pagination count={1} start={0} total={total} goTo={console.log} />
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
                        <img src="https://wsdb.xyz/icons/1423.webp" />
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