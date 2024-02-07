import React, { useCallback, useState } from 'react'
import { Image, Container, Row, Stack, Table, Button } from 'react-bootstrap'
import { IItem, IListItemsParams  } from "./interfaces"
import StandardPagination from './StandardPagination';
import { listItems } from './api';
import { isApiError } from './typeCheck';
import FilterItems, { IFilterItemsProps } from './FilterItems'
import { getAssetUrl } from './utils';

export interface ISelectItemProps {
    itemSelected(item: IItem): any
}

const SelectItem : React.FC<ISelectItemProps> = ({
    itemSelected
}) => {
    const [items, setItems] = useState<IItem[]>([]);
    const [total, setTotal] = useState(1);
    const [searchParams, setSearchParams] = useState<IListItemsParams>({
        start: 0,
        count: 10
    });

    const searchItems = useCallback((p: IListItemsParams) => {
        listItems(p)
        .then(r => {
            if (isApiError(r)) {
                alert(`Error on listing items. Message: ${r.message}`)
                return;
            }
            if (r.total !== total)
                setTotal(r.total)
            setItems(r.data)
        });
    }, [total, setTotal]);

    const handleFilter = useCallback<IFilterItemsProps['onFilter']>(
        (filter) => {
            setSearchParams(p => {
                p.start = 0;
                return {...p, ...filter};
            });
            searchItems({...searchParams, ...filter})
        },
        [setSearchParams, searchItems]
    );

    return (
        <Container>
            <Stack gap={3}>
                <FilterItems onFilter={handleFilter} />
                <Row>
                    <Table>
                        <thead>
                            <tr>
                                <th>Icon</th>
                                <th>Name</th>
                                <th>Level</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {items.map(i => 
                                (<tr className='mb-2' key={i.id}>
                                    <td>
                                        {
                                            i.asset !== null &&
                                            <Image src={getAssetUrl(i.asset)}/>
                                        }
                                    </td>
                                    <td>{i.name}</td>
                                    <td>{i.level}</td>
                                    <td><Button variant='success' onClick={() => itemSelected(i)}>Select</Button></td>
                                </tr>)
                            )}
                        </tbody>
                    </Table>
                </Row>
                <StandardPagination 
                    total={total} 
                    count={searchParams.count} 
                    start={searchParams.start} 
                    goTo={(index: number) => {
                        setSearchParams(p => ({...p, start: index}))
                        searchItems({...searchParams, start: index})
                    }}
                />
            </Stack>
        </Container>
    )
}

export default SelectItem;