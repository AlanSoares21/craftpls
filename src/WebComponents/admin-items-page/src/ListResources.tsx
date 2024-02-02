import { Badge, Button, Col, Container, Image, Row, Table } from "react-bootstrap";
import { ICraftResource, IItem } from "./interfaces";
import { getAssetUrl } from "./utils";
import { useCallback, useEffect, useState } from "react";
import { deleteItemResource, listItemResources } from "./api";
import { isApiError } from "./typeCheck";
import AddResourceModal from "./AddResourceModal";
import UpdateResourceModal from "./UpdateResourceModal";

export interface IListResourcesProps {
    item: IItem
    close(): any
}

const ListResources: React.FC<IListResourcesProps> = ({
    item, close
}) => {
    const [openAddModal, setOpenAddModal] = useState(false);
    const [resources, setResources] = useState<ICraftResource[]>([]);
    const [resourceToUpdate, setResourceToUpdate] = useState<ICraftResource>();

    const searchResources = useCallback(() => {
        listItemResources(item.id)
        .then(r => {
            if (isApiError(r)) {
                alert(`Error listing resources for ${item.name}(${item.id}). Message: ${r.message}`);
                return;
            }
            setResources(r.data);
        })
    }, []);

    const deleteResource = useCallback((resource: ICraftResource) => {
        deleteItemResource(resource)
        .then(r => {
            if (isApiError(r)) {
                alert(`Error on remove resource. Message: ${r.message}`);
                return;
            }
            if (r)
                searchResources();
        });
    }, [searchResources]);

    useEffect(() => {
        searchResources();
    }, []);

    return (<Container>
        <Row className="mb-2">
            <Col xs={2}>
                {
                    item.asset !== undefined &&
                    <Image src={getAssetUrl(item.asset)} width='100%' />
                }
            </Col>
            <Col>
                <Row>
                    <Col>
                        <h2>{item.name}</h2>
                    </Col>
                    <Col xs={1}>
                        <Badge bg='danger' onClick={close}>X</Badge>
                    </Col>
                </Row>
                Level: {item.level}
            </Col>
        </Row>
        <Button variant="success" onClick={() => setOpenAddModal(true)}>Add</Button>
        <Row>
            <Table>
                <thead>
                    <tr>
                        <th colSpan={5}>Resources</th>
                    </tr>
                    <tr>
                        <th>Icon</th>
                        <th>Name</th>
                        <th>Level</th>
                        <th>Ammount</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {resources.map(r => (<tr key={r.id}>
                        <td>
                            {
                                r.resource.asset !== undefined &&
                                <Image src={getAssetUrl(r.resource.asset)} />
                            }
                        </td>
                        <td>{r.resource.name}</td>
                        <td>{r.resource.level}</td>
                        <td>{r.amount}</td>
                        <td>
                            <Row>
                                <Col md='auto'>
                                    <Button 
                                        variant='warning' 
                                        onClick={() => setResourceToUpdate(r)}>
                                        Update
                                    </Button>
                                </Col>
                                <Col md='auto'>
                                    <Button
                                        variant='danger' 
                                        onClick={() => deleteResource(r)}>
                                        Delete
                                    </Button>
                                </Col>
                            </Row>
                        </td>
                    </tr>))}
                </tbody>
            </Table>
        </Row>
        <AddResourceModal 
            open={openAddModal} 
            item={item}
            onClose={() => setOpenAddModal(false)} 
            onSuccess={() => {
                setOpenAddModal(false)
                searchResources()
            }}
        />
        <UpdateResourceModal 
            open={resourceToUpdate !== undefined}
            craftResource={resourceToUpdate}
            onClose={() => setResourceToUpdate(undefined)}
            onSubmit={() => {
                searchResources();
                setResourceToUpdate(undefined);
            }}
        />
    </Container>);
}

export default ListResources;