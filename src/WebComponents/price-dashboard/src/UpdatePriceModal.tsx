import { MouseEventHandler, useCallback, useRef, useState } from "react";

export interface IUpdatePriceModalProps {
    id: string;
    manufacturer: string;
}

const UpdatePriceModal : React.FC<IUpdatePriceModalProps> = ({
    id, manufacturer
}) => {
    const closeRef = useRef<HTMLButtonElement>(null)
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
        if (price <= 0) {
            setError("*You should set a valid craft price")
            return;
        }
        setError("")
        closePanel()
    }, [id, manufacturer, closePanel, price])

    return <div className="modal fade" id={id} aria-labelledby="exampleModalLabel" tabIndex={-1} aria-hidden="true">
        <div className="modal-dialog">
            <div className="modal-content">
            
                <div className="modal-header">
                    <h5 className="modal-title">Set the new craft price</h5>
                    <button 
                        type="button" 
                        className="btn-close" 
                        data-bs-dismiss="modal" 
                        aria-label="Close">    
                    </button>
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

export default UpdatePriceModal