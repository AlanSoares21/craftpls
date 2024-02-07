import { useContext, useState } from "react";
import { Button, Form, Stack } from "react-bootstrap";
import { handleNumericInput } from "./utils";
import { IFilterItems } from "./interfaces";
import { CommomDataContext } from "./CommomDataContext";

export interface IFilterItemsProps {
    onFilter(filter: IFilterItems): void
}

const FilterItems : React.FC<IFilterItemsProps> = ({
    onFilter
}) => {
    const commomData = useContext(CommomDataContext);
    const [name, setName] = useState<string>();
    const [maxLevel, setMaxLevel] = useState<number>();
    const [minLevel, setMinLevel] = useState<number>();
    const [categoryId, setCategoryId] = useState<number>();

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
                    >
                        <option value={-1}>Any</option>
                        {
                            commomData.static.categories.map(c => (
                                <option key={c.id} value={c.id}>{c.name}</option>
                            ))
                        }
                    </Form.Select>
                </div>
            </Stack>
            <Button 
                variant="primary" 
                onClick={() => onFilter({name, minLevel, maxLevel, categoryId})}
            >
                Filter items
            </Button>
        </form>
    );
}

export default FilterItems;