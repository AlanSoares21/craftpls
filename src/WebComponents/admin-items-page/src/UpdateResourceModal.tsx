import { Button, Form, Modal } from "react-bootstrap"
import { ICraftResource } from "./interfaces"
import { handleNumericInput } from "./utils"
import { useCallback, useEffect, useState } from "react"
import { updateItemResource } from "./api"
import { isApiError } from "./typeCheck"

export interface IUpdateResourceModalProps {
    open: boolean
    onClose(): any
    onSubmit(amount: ICraftResource['amount']): any
    craftResource?: ICraftResource
}

const UpdateResourceModal: React.FC<IUpdateResourceModalProps> = ({
    open, onClose, craftResource, onSubmit
}) => {
    const [amount, setAmount] = useState<number | undefined>();

    const handleSubmit = useCallback(() => {
        if (craftResource === undefined) {
            alert("Craft resource data is undefined.");
            return;
        }
        if (amount === undefined || amount < 1) {
            alert("amount should be greater than zero.");
            return;
        }
        updateItemResource(craftResource.id, {amount})
        .then(r => {
            if (isApiError(r))
                alert(`Error updating resource. Message: ${r.message}`)
            else if (r) 
                onSubmit(amount);
        })
    } , [onSubmit, amount, craftResource]);

    useEffect(() => {
        if (craftResource)
            setAmount(craftResource.amount)
    }, [craftResource])

    return (<Modal show={open} onHide={onClose} size='lg'>
        <Modal.Header>
            <Modal.Title>
                Update resource
            </Modal.Title>
        </Modal.Header>
        <Modal.Body>
            <Form.Label htmlFor="txtAmount">Amount: </Form.Label>
            <Form.Control 
                id='txtAmount' 
                type="number" 
                defaultValue={amount}
                onChange={handleNumericInput(setAmount)}
            />
        </Modal.Body>
        <Modal.Footer>
            <Button variant="secondary" onClick={onClose}>Cancel</Button>
            <Button variant="warning" onClick={handleSubmit}>Update</Button>
        </Modal.Footer>
    </Modal>)
}

export default UpdateResourceModal;
