import { useCallback, useEffect, useState } from "react";
import { ICraftResource, IItemPrice } from "./interfaces";
import { Badge, Button, Col, Image, Modal, Row, Table } from "react-bootstrap";
import { checkPriceResources, listItemResources, listPrices } from "./api";
import { getAssetUrl } from "./utils";
import { isApiError } from "./typeCheck";

export interface ICheckResourcesModalProps {
    open: boolean
    onClose(): any
    onSuccess(): any
    changePrice(price: IItemPrice): any
    price?: IItemPrice
    manufacturer: string
}

const CheckResourcesModal: React.FC<ICheckResourcesModalProps> = ({
    open, price, manufacturer, changePrice, onSuccess, onClose
}) => {
    const [resources, setResources] = useState<ICraftResource[]>([]);
    const [missingResources, setMissingResources] = useState<ICraftResource[]>([]);
    const [resourcesPrices, setResourcesPrices] = useState<IItemPrice[]>([]);

    useEffect(() => {
        setMissingResources(resources.filter(r => !resourcesPrices.some(p => p.itemId === r.resourceId)))
    }, [resourcesPrices, resources])

    useEffect(() => {
        if (price !== undefined) {
            listItemResources(price.itemId).then(r => setResources(r.data));
            listPrices(manufacturer, {resourcesOf: price.itemId, start: 0, count: 200})
            .then(r => setResourcesPrices(r.data));
        }
        else {
            setResources([])
            setResourcesPrices([])
        }
    }, [price])

    const handleSubmit = useCallback(() => {
        if (price === undefined) {
            alert("price is undefined")
            return
        }
        checkPriceResources(price.id).then(r => {
            if (isApiError(r)) {
                alert(r.message)
                return;
            }
            onSuccess()
        })
    }, [price, onSuccess])
    /**
     * listar os recursos e os preços desses recursos exibir o total price do recurso atual
     * se tem um novo item como resource, impede de dar o check
     * se tem o preço para todos, permite dar o check
     */
    return (<Modal size="lg" show={open}>
        <Modal.Header>
            <Modal.Title>
                Resources for {price && price.item.name}
            </Modal.Title>
        </Modal.Header>
        <Modal.Body>
                {
                    price && 
                    <Row>
                        <Col xs={1}>
                            <Image 
                                src={
                                    price.item.asset && 
                                    getAssetUrl(price.item.asset)
                                } 
                            />
                        </Col>
                        <Col>{price.item.name}</Col>
                        <Col>Craft Price: {price.price}</Col>
                        <Col><b>Total Price: {price.totalPrice}</b></Col>
                    </Row>
                }
                {
                    missingResources.length > 0 ?
                    <Table>
                        <thead>
                            <tr>
                                <th colSpan={3}>You should add price for this item before check this price</th>
                            </tr>
                            <tr>
                                <th>Icon</th>
                                <th>Name</th>
                                <th>Ammount</th>
                            </tr>
                        </thead>
                        <tbody>
                            {
                                missingResources.map(r => (<tr key={r.id}>
                                        <td><Image src={r.resource.asset && getAssetUrl(r.resource.asset)} /></td>
                                        <td>{r.resource.name}</td>
                                        <td>{r.amount}</td>
                                    </tr>)
                                )
                            }
                        </tbody>
                    </Table>
                    :
                    <Table>
                        <thead>
                            <tr>
                                <th colSpan={5}>Resources</th>
                            </tr>
                            <tr>
                                <th>Icon</th>
                                <th>Name</th>
                                <th>Total Price</th>
                                <th>Amount</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {
                                resourcesPrices.map(p => {
                                    const resourceIndex = resources.findIndex(r => r.resourceId === p.itemId);
                                    return (<tr key={p.id}>
                                        <td><Image src={p.item.asset && getAssetUrl(p.item.asset)} /></td>
                                        <td>{p.item.name}</td>
                                        <td>{p.totalPrice}</td>
                                        <td>{resourceIndex !== -1 && resources[resourceIndex].amount}</td>
                                        <td>{
                                            !p.resourcesChanged ?
                                            <Badge bg="success">OK</Badge>
                                            :
                                            <Badge bg='warning' onClick={() => changePrice(p)}>Check this price first</Badge>
                                        }</td>
                                    </tr>)
                                })
                            }
                        </tbody>
                    </Table>
                }
        </Modal.Body>
        <Modal.Footer>
            <Button variant="secondary" onClick={onClose}>Close</Button>
            <Button variant="success" disabled={missingResources.length > 0} onClick={handleSubmit}>Prices are Ok</Button>
        </Modal.Footer>
    </Modal>);
}

export default CheckResourcesModal;