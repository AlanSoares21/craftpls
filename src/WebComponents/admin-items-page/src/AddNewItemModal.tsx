import React, { useCallback, useContext, useState } from "react";
import { Button, Form, Modal, Image, Stack } from "react-bootstrap";
import { CommomDataContext } from "./CommomDataContext";
import { getAssetUrl, handleNumericInput } from "./utils";
import SelectAsset from "./SelectAsset";
import { IAsset, IAttribute, IItemToAdd } from "./interfaces";
import { addItem } from "./api";
import { isApiError } from "./typeCheck";

export interface IAddNewItemModalProps {
    isOpen: boolean
    close(): any
    onItemAdded(): any
}

const AddNewItemModal: React.FC<IAddNewItemModalProps> = ({
    isOpen, close, onItemAdded
}) => {
    const commomData = useContext(CommomDataContext)
    const [categoryId, setCategoryId] = useState<number>()
    const [level, setLevel] = useState<number>()
    const [name, setName] = useState<string>()
    const [asset, setAsset] = useState<IAsset>()
    const [attributes, setAttributes] = useState<{
        attribute: IAttribute
        value: number
    }[]>([])
    const [names, setNames] = useState<{culture: string, value: string}[]>([])
    const [cultureToAdd, setCultureToAdd] = useState<string>("pt")

    const handleAddItem = useCallback(() => {
        console.log(attributes)
        if (name === undefined) {
            alert("Name should be provided")
            return
        }

        addItem({name, level, categoryId, assetId: asset?.id, 
            attributes: attributes.map(a => 
                ({attributeId: a.attribute.id, value: a.value})
            ),
            namesByCulture: names.reduce((prev, v) => {
                prev[v.culture] = v.value
                return prev
            }, {} as IItemToAdd['namesByCulture'])
        }).then(r => {
            if (isApiError(r))
                alert(`Error on add item ${name}. Message: ${r.message}`)
            else {
                setName(undefined)
                setLevel(undefined)
                setCategoryId(undefined)
                setAsset(undefined)
                setAttributes([])
                setNames([])
                onItemAdded()
            }
        })
    }, [categoryId, level, name, asset, attributes, names, onItemAdded])    

    return (<Modal show={isOpen} onHide={close} size="lg">
        <Modal.Header>
            <Modal.Title>Add item</Modal.Title>
        </Modal.Header>
        <Modal.Body>
            <div>
                <Form.Label htmlFor='itemName'>Item name</Form.Label>
                <Form.Control 
                    name='itemName' 
                    onChange={ev => setName(ev.currentTarget.value)}
                />
            </div>
            <div className="mt-1 mb-1">
                <Form.Label htmlFor="culture">Culture to add</Form.Label>
                <Form.Control 
                    name='culture' 
                    value={cultureToAdd} 
                    onChange={ev => setCultureToAdd(ev.currentTarget.value)}
                />
                <Button 
                    className="mt-1"
                    variant='success'
                    onClick={() => {
                        setNames([...names, {culture: cultureToAdd, value: "Name"}])
                    }}
                >
                    Add culture
                </Button>
            </div>
            <div className="mb-1">
                {
                    names.map((n, i) => (
                        <div key={n.culture} className="mb-1">
                            <Form.Label>Name in {n.culture}</Form.Label>
                            <Form.Control 
                                defaultValue={n.value} 
                                onChange={ev => {
                                    const newNames = [...names]
                                    newNames[i].value = ev.currentTarget.value
                                    setNames(newNames)
                                }}
                            />
                        </div>
                    ))
                }
            </div>
            <div>
                <Form.Label htmlFor='level'>Level</Form.Label>
                <Form.Control
                    name='level'
                    onChange={handleNumericInput(setLevel)}
                />
            </div>
            <div>
                <Form.Label htmlFor="selectCategory">Category</Form.Label>
                <Form.Select 
                    id="selectCategory"
                    onChange={ev => {
                        const id = parseInt(ev.currentTarget.value);
                        if (id > 0)
                            setCategoryId(id)
                        else
                            setCategoryId(undefined);
                    }}
                    defaultValue={categoryId}
                >
                    <option value={-1}>Any</option>
                    {
                        commomData.static.categories.map(c => (
                            <option
                                key={c.id}
                                value={c.id}
                            >
                                {c.name}
                            </option>
                        ))
                    }
                </Form.Select>
            </div>
            <Form.Group className="border mt-2 mb-1">
                {
                    asset === undefined ?
                    <>
                    <Form.Label>Select an asset</Form.Label>
                    <SelectAsset onSelect={setAsset} />
                    </>
                    :
                    <>
                        <Image src={getAssetUrl(asset)} />
                        <Button 
                            variant="warning"
                            onClick={() => setAsset(undefined)}
                        >
                            Remove asset
                        </Button>
                    </>
                }
            </Form.Group>
            <Stack>
                {
                    attributes.map((_, i) => (
                        <div key={`att${i}`} className="mb-2">
                            <Form.Label htmlFor="selectAttribute">Attribute</Form.Label>
                            <Form.Select
                                id='selectAttribute'
                                onChange={ev => {
                                    const index = parseInt(ev.currentTarget.value);
                                    const atts = [...attributes]
                                    atts[i].attribute = 
                                        commomData.static.attributes[index]
                                    setAttributes(atts)
                                }}
                            >
                                {
                                    commomData.static.attributes.map((a, index) => (
                                        <option key={a.id} value={index}>
                                            {a.name}
                                        </option>
                                    ))
                                }
                            </Form.Select>
                            <Form.Control
                                onChange={ev => {
                                    const value = parseFloat(ev.currentTarget.value)
                                    const atts = [...attributes]
                                    atts[i].value = value
                                    setAttributes(atts)
                                }}
                            />
                        </div>
                    ))
                }
                <Button
                    onClick={() => setAttributes([
                        ...attributes, 
                        {
                            value: 0,
                            attribute: commomData.static.attributes[0]
                        }
                    ])}
                    variant="success"
                >
                    Add attribute
                </Button>
            </Stack>
        </Modal.Body>
        <Modal.Footer>
            <Button
                variant="secondary"
                onClick={() => {
                    setName(undefined)
                    setLevel(undefined)
                    setCategoryId(undefined)
                    setAsset(undefined)
                    setAttributes([])
                    setNames([])
                    close()
                }}
            >
                Cancel
            </Button>
            <Button
                variant="success"
                onClick={handleAddItem}
            >
                Add item
            </Button>
        </Modal.Footer>
    </Modal>);
}

export default AddNewItemModal;