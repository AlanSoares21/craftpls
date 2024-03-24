import { useState } from 'react'
import { Button, Col, Container, Row} from 'react-bootstrap'
import SelectItem from './SelectItem';
import { IFilterItems, IItem } from './interfaces';
import ShowItemDetails from './ShowItemDetails';
import { CommomDataProvider } from './CommomDataContext';
import AddNewItemModal from './AddNewItemModal';


function App() {
    const [openAddItem, setOpenAddItem] = useState(false)
    const [item, setItem] = useState<IItem>();
    const [filterData, setFilterData] = useState<IFilterItems>();

    return <CommomDataProvider>
        <Container> {
            item === undefined ?
                <>
                    <Row className="justify-content-md-center">
                        <Col md='1'></Col>
                        <Col>
                            <Button 
                                onClick={() => setOpenAddItem(true)}
                            >
                                    Add item
                            </Button>
                        </Col>
                        <Col md='1'></Col>
                    </Row>
                    <SelectItem itemSelected={setItem} allowDelete filterData={filterData} onFilterChange={setFilterData} />
                </>
            :
                <ShowItemDetails item={item} close={() => setItem(undefined)} />
        } </Container>
        <AddNewItemModal 
            isOpen={openAddItem} 
            close={() => setOpenAddItem(false)} 
            onItemAdded={() => setOpenAddItem(false)}
        />
    </CommomDataProvider>;
}

export default App
