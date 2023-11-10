import { MouseEventHandler, useCallback, useRef, useState } from "react";
import { IItemPrice } from "./interfaces";
import { updatePrice } from "./api";

export interface IUpdatePriceModalProps {
    id: string;
    manufacturer: string;
    itemsPrices: {
        [id: IItemPrice['id']]: IItemPrice
    };
    onUpdate(): void;
}

const UpdatePriceModal : React.FC<IUpdatePriceModalProps> = ({
    id, manufacturer, itemsPrices, onUpdate
}) => {
    const [pricesUpdated, setPricesUpdated] = useState<number>()
    const closeRef = useRef<HTMLButtonElement>(null)
    const [price, setPrice] = useState(0)

    const [errorMessage, setError] = useState("")

    const closePanel = useCallback(() => {
        if (closeRef && closeRef.current) {
            closeRef.current.click()
            return;
        }
    }, [closeRef])

    const onSave = useCallback<MouseEventHandler<HTMLButtonElement>>(
        async () => {
        if (price <= 0) {
            setError("*You should set a valid craft price")
            return;
        }
        let success = 0;
        let fail = 0;
        setPricesUpdated(0)
        for (const priceId in itemsPrices) {
            await updatePrice(priceId, {price})
            .then(() => {
                success++;
            })
            .catch(() => {
                fail++;
            })
            .finally(() => {
                setPricesUpdated(fail + success)
            })
        }
        setPricesUpdated(undefined)
        if (fail === 0) {
            setError("")
            closePanel()
        } else {
            setError(`${success} items prices updated. ${fail} failed`)
        }
        onUpdate()
    }, [manufacturer, itemsPrices, closePanel, price, onUpdate])

    return <div className="modal fade" id={id} aria-labelledby="exampleModalLabel" tabIndex={-1} aria-hidden="true">
        <div className="modal-dialog">
            <div className="modal-content">
            
                <div className="modal-header">
                    <h5 className="modal-title">Set the new craft price</h5>
                    {
                        pricesUpdated === undefined &&
                        <button 
                            type="button" 
                            className="btn-close" 
                            data-bs-dismiss="modal" 
                            aria-label="Close">    
                        </button>
                    }
                </div>
                
                <div className="modal-body">
                    <form>
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
                    </form>
                </div>
                
                <div className="modal-footer"> 
                    <div className="col-1">
                        {errorMessage}
                    </div>
                    <div className="col-4">
                        <button 
                            ref={closeRef}
                            type="button" 
                            className="btn btn-danger" 
                            data-bs-dismiss="modal">
                            Close
                        </button>
                    </div>
                    <div className="col-6">
                    {
                        pricesUpdated === undefined ?
                        <button 
                            type="button" 
                            className="btn btn-success"
                            onClick={onSave}>
                            Save
                        </button>
                        :
                        <div
                            className="progress" 
                            role="progressbar"
                            aria-valuenow={pricesUpdated / Object.keys(itemsPrices).length * 100} 
                            aria-valuemin={0} 
                            aria-valuemax={100}
                        >
                            <div 
                                className="progress-bar" 
                                style={{width: `${pricesUpdated / Object.keys(itemsPrices).length * 100}%`}}
                            >
                            </div>
                        </div>
                    }
                    </div>
                    
                </div>

            </div>
        </div>
    </div>
}

export default UpdatePriceModal