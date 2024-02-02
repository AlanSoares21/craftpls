import React, { useCallback, useState } from "react";
import { Button, Col, Modal, Row, Image, Badge, Stack, Form } from "react-bootstrap";
import { IItem } from "./interfaces";
import SelectItem from "./SelectItem";
import { getAssetUrl, handleNumericInput } from "./utils";
import { addItemResource } from "./api";
import { isApiError } from "./typeCheck";

export interface IAddResourceModalProps {
    item: IItem
    open: boolean
    onClose(): any
    onSuccess(): any
}

const AddResourceModal: React.FC<IAddResourceModalProps> = ({
    open, onClose, item, onSuccess
}) => {
    const [errorMesage, setError] = useState("");
    const [resourceSelected, setResource] = useState<IItem>()

    const [amount, setAmount] = useState<number | undefined>(1);

    const handleAddResource = useCallback(() => {
        if (resourceSelected === undefined) {
            setError("You should select an item to use as resource");
            return;
        }
        if (amount === undefined || amount < 1) {
            setError("The amount should be greater than one");
            return;
        }
        addItemResource({
            itemId: item.id,
            resourceId: resourceSelected.id,
            amount
        }).then(r => {
            if (isApiError(r)) {
                setError(`Error on add resource. Message: ${r.message}`);
                return;
            }
            setError("");
            setResource(undefined);
            setAmount(1);
            onSuccess();
        })
    }, [item, resourceSelected, setResource, amount, setAmount, onSuccess, setError]);

    return (<Modal show={open} onHide={onClose} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>
            {
                resourceSelected === undefined ?
                    "Select an item"
                :
                    "Insert the amount"

            }
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
            <Row>
            {
                resourceSelected === undefined ?
                    <SelectItem itemSelected={setResource} />
                :
                    <>
                        <Col xs={1}>
                            {
                                resourceSelected.asset !== null &&
                                <Image src={getAssetUrl(resourceSelected.asset)} />
                            }
                        </Col>
                        <Col>
                            <Stack>
                                <Row>
                                    <Col>
                                        <h5>{resourceSelected.name}</h5>
                                    </Col>
                                    <Col xs={1}>
                                        <Badge bg='warning' onClick={() => setResource(undefined)}>X</Badge>
                                    </Col>
                                </Row>
                                <div>
                                    Level: {resourceSelected.level}
                                </div>
                            </Stack>
                        </Col>
                    </> 
            }
            </Row>
            <Form>
                <Form.Label htmlFor="txtAmount">Amount</Form.Label>
                <Form.Control 
                    disabled={resourceSelected === undefined}
                    id="txtAmount" 
                    type="number" 
                    value={amount} 
                    onChange={handleNumericInput(setAmount)}
                />
            </Form>
        </Modal.Body>
        <Modal.Footer>
            <p>
                {errorMesage}
            </p>
            <Button variant="secondary" onClick={onClose}>
            Cancel
            </Button>
            <Button variant="success" onClick={handleAddResource}>
            Add Resource
            </Button>
        </Modal.Footer>
    </Modal>);
}

export default AddResourceModal;