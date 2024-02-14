import { useState } from 'react'
import { Container} from 'react-bootstrap'
import SelectItem from './SelectItem';
import { IItem } from './interfaces';
import ListResources from './ListResources';
import { CommomDataProvider } from './CommomDataContext';


function App() {
    const [item, setItem] = useState<IItem>();

    return <CommomDataProvider>
        <Container> {
            item === undefined ?
                <SelectItem itemSelected={setItem} allowDelete />
            :
                <ListResources item={item} close={() => setItem(undefined)} />
        } </Container>
    </CommomDataProvider>;
}

export default App
