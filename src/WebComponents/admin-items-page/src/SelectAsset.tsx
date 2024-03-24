import React, { useCallback, useState } from "react";
import { Button, Col, Container, Form, ListGroup, Row, Image } from "react-bootstrap";
import { listAssets } from "./api";
import StandardPagination from "./StandardPagination";
import { IAsset, IStandardList, IStandardPaginationParams } from "./interfaces";
import { isApiError } from "./typeCheck";
import { getAssetUrl } from "./utils";

export interface ISelectAssetProps {
    onSelect(asset: IAsset): any
}

const SelectAsset: React.FC<ISelectAssetProps> = ({
    onSelect
}) => {
    const [itemName, setItemName] = useState<string>()
    const [endpoint, setEndpoint] = useState<string>()
    const [unusedAssets, setUnusedAssets] = useState<boolean>()
    const [pagination, setPagination] = useState<IStandardPaginationParams>({
        count: 5,
        start: 0
    })
    const [assets, setAssets] = useState<IStandardList<IAsset>>({
        total: 0,
        count: 0,
        data: [],
        start: 0
    })

    const handleListAssets = useCallback((p: IStandardPaginationParams) => {
        listAssets({...p, itemName, endpoint, unusedAssets})
        .then(r => {
            if (isApiError(r))
                alert(`Error on list assets. Message: ${r.message}`)
            else
                setAssets(r)
        })
    }, [itemName, endpoint, unusedAssets, setAssets])
    
    return <Container>
        <Row className="mb-2">
            <Col>
                <Form.Label htmlFor="itemName">Item name</Form.Label>
                <Form.Control 
                    name="itemName"
                    onChange={ev => setItemName(ev.currentTarget.value)}
                />
            </Col>
            <Col>
                <Form.Label htmlFor="endpoint">Endpoint</Form.Label>
                <Form.Control 
                    name="endpoint"
                    onChange={ev => setEndpoint(ev.currentTarget.value)}
                />
            </Col>
            <Col>
                <Form.Check
                    type="checkbox"
                    label="Filter by used/unsed"
                    onChange={() => {
                        if (unusedAssets === undefined)
                            setUnusedAssets(true)
                        else
                            setUnusedAssets(undefined)
                    }}
                />
                <Form.Check 
                    type="switch"
                    label="Unsed assets"
                    disabled={unusedAssets === undefined}
                    onChange={() => {
                        setUnusedAssets(!unusedAssets)
                    }}
                    checked={unusedAssets}
                />
            </Col>
        </Row>
        <Row className="mb-1">
            <Button 
                variant="primary"
                onClick={() => handleListAssets(pagination)}
            >
                Search assets
            </Button>
        </Row>
        <Row>
            <ListGroup>
                {
                    assets.data.map(asset => (
                        <ListGroup.Item
                            key={asset.id}
                        >
                            <Row>
                                <Col sm lg xl md='1' className="mb-1">
                                    <Image src={getAssetUrl(asset)}/>
                                </Col>
                                <Col className="mb-1">
                                    <div className="text-wrap">{asset.endpoint}</div>
                                </Col>
                                <Col>
                                    <Button
                                        variant="success"
                                        onClick={() => onSelect(asset)}
                                    >
                                        Select
                                    </Button>
                                </Col>
                            </Row>
                        </ListGroup.Item>
                    ))
                }
            </ListGroup>
        </Row>
        <Row>
            <StandardPagination 
                total={assets.total}
                count={pagination.count} 
                start={pagination.start} 
                goTo={start => {
                    const newPaginationParams = {...pagination, start};
                    setPagination(newPaginationParams)
                    handleListAssets(newPaginationParams)
                }}
            />
        </Row>
    </Container>
}

export default SelectAsset