import { useState } from 'react'
import { Container} from 'react-bootstrap'
import SelectItem from './SelectItem';
import { IFilterItems, IItem } from './interfaces';
import ListResources from './ListResources';
import { CommomDataProvider } from './CommomDataContext';


function App() {
    const [item, setItem] = useState<IItem>();
    const [filterData, setFilterData] = useState<IFilterItems>();

    return <CommomDataProvider>
        <Container> {
            item === undefined ?
                <SelectItem itemSelected={setItem} allowDelete filterData={filterData} onFilterChange={setFilterData} />
            :
                <ListResources item={item} close={() => setItem(undefined)} />
        } </Container>
    </CommomDataProvider>;
}

export default App
