import React, { MouseEventHandler, useCallback, useRef, useState } from "react";
import SelectItem from "./SelectItem";
import { IItem } from "./interfaces";
import { addPrice } from "./api";

export interface INewPriceModalProps {
    id: string;
    manufacturer: string;
}

const NewPriceModal : React.FC<INewPriceModalProps> = ({
    id, manufacturer
}) => {
    const closeRef = useRef<HTMLButtonElement>(null)
    const [item, setItem] = useState<IItem>()
    const [price, setPrice] = useState(0)

    const [errorMessage, setError] = useState("")

    const closePanel = useCallback(() => {
        if (closeRef && closeRef.current) {
            closeRef.current.click()
            console.log("finished close modal");
            return;
        }
    }, [closeRef])

    const onSave = useCallback<MouseEventHandler<HTMLButtonElement>>(
        () => {
        if (item === undefined) {
            setError("*You should select an item before try save")
            return;
        }
        if (price <= 0) {
            setError("*You should set a craft price for the item")
            return;
        }
        setError("")
        addPrice({
            itemId: item.id,
            manufacturerId: manufacturer,
            price
        }).then(() => {
            
        })
        .finally(closePanel)
    }, [id, manufacturer, closePanel, item, price])

    return <div className="modal fade" id={id} tabIndex={-1} aria-hidden="true">
        <div className="modal-dialog modal-lg">
            <div className="modal-content">
            
                <div className="modal-header">
                    <h5 className="modal-title">Add price</h5>
                    <button 
                        type="button" 
                        className="btn-close" 
                        data-bs-dismiss="modal" 
                        aria-label="Close">    
                    </button>
                </div>
                
                <div className="modal-body">
                    <form onSubmit={ev => ev.preventDefault()}>
                        <div className="mb-1">
                            <SelectItem 
                                manufacturer={manufacturer} 
                                itemSelected={setItem}
                            />
                        </div>
                        <div>
                            <label htmlFor="txtPrice" className="form-label">Craft price</label>
                            <input 
                                id="txtPrice" 
                                className="form-control" 
                                type="number" 
                                min={0}
                                onChange={(ev) => {
                                   const price = parseInt(ev.target.value);
                                    setPrice(price)
                                }}
                            />
                        </div>
                    </form>
                </div>
                
                <div className="modal-footer">
                    <div className="col">
                        {errorMessage}
                    </div>
                    <button 
                        ref={closeRef}
                        type="button" 
                        className="btn btn-danger" 
                        data-bs-dismiss="modal">
                        Cancel
                    </button>
                    <button 
                        type="button" 
                        className="btn btn-success"
                        onClick={onSave}>
                        Save
                    </button>
                </div>

            </div>
        </div>
    </div>
}

export default NewPriceModal;