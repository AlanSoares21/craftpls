import { useState } from "react";
import { Button, Form, Stack } from "react-bootstrap";
import { handleNumericInput } from "./utils";

export interface IFilterItemsProps {
    onFilter(searchTerm?: string, min?: number, max?: number): void
}

const FilterItems : React.FC<IFilterItemsProps> = ({
    onFilter
}) => {

    const [name, setName] = useState<string>();
    const [maxLevel, setMaxLevel] = useState<number>();
    const [minLevel, setMinLevel] = useState<number>();

    return (
        <form>
            <Stack direction="horizontal" gap={3} className="mb-1">
                <div>
                    <Form.Label htmlFor="txtName">Name</Form.Label>
                    <Form.Control 
                        type="text" 
                        id="txtName" 
                        onChange={ev => {
                            setName(ev.target.value)
                        }}
                        defaultValue={name}
                    />
                </div>
                <div>
                    <Form.Label htmlFor="txtMaxLevel">Max level</Form.Label>
                    <Form.Control 
                        id="txtMaxLevel" 
                        type="number" 
                        onChange={handleNumericInput(setMaxLevel)}
                    />
                </div>
                <div>
                    <Form.Label htmlFor="txtMinLevel">Min level</Form.Label>
                    <Form.Control 
                        id="txtMinLevel" 
                        type="number" 
                        onChange={handleNumericInput(setMinLevel)}
                    />
                </div>
            </Stack>
            <Button 
                variant="primary" 
                onClick={() => onFilter(name, minLevel, maxLevel)}
            >
                Filter items
            </Button>
        </form>
    );
}

export default FilterItems;