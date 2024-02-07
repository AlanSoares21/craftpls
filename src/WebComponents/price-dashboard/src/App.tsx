import { useEffect, useState } from 'react';
import './App.css'
import ListItems from './ListPrices'
import NewPriceModal from './NewPriceModal'
import { CommomDataProvider } from './CommomDataContext';

const addPriceModalId = "addPriceModal"

function App() {
  const [manufacturer, setManufacturer] = useState("")

  useEffect(() => {
    if (import.meta.env.DEV) {
      setManufacturer(import.meta.env.VITE_ManufacturerId + "")
    }
    else {
      const path = location.pathname.split('/')
      setManufacturer(path[path.length - 1])
    }
  }, [])

  return (
    <CommomDataProvider>
      <section className='container'>
        <NewPriceModal 
          id={addPriceModalId} 
          manufacturer={manufacturer} 
        />
        <div className='row mb-1'>
          <button 
            type='button' 
            className='btn btn-success' 
            data-bs-toggle="modal" 
            data-bs-target={"#" + addPriceModalId}
            >
            Add Price
          </button>
        </div>
        <div className='row mb-1'>
          <ListItems manufacturer={manufacturer} />
        </div>
      </section>
    </CommomDataProvider>
  )
}

export default App
