import { useState } from 'react'
import { Container} from 'react-bootstrap'
import SelectItem from './SelectItem';
import { IItem } from './interfaces';
import ListResources from './ListResources';


function App() {
    const [item, setItem] = useState<IItem>();

    return <Container> {
            item === undefined ?
                <SelectItem itemSelected={setItem} />
            :
                <ListResources item={item} close={() => setItem(undefined)} />
        }
    </Container>;
}

export default App
