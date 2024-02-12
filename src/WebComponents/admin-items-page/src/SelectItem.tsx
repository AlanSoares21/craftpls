import React, { useCallback, useState } from 'react'
import { Image, Container, Row, Stack, Table, Button, Col } from 'react-bootstrap'
import { IItem, IListItemsParams  } from "./interfaces"
import StandardPagination from './StandardPagination';
import { deleteItems, listItems } from './api';
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

    const handleDeleteItem = useCallback((itemId: number) => {
        deleteItems(itemId)
        .then(r => {
            if (isApiError(r))
                alert(`Error on ${r.message}`);
            if (r)
                searchItems(searchParams);
            else
                alert(`Unknowed error on delete item ${itemId}`)
        })
    }, [searchItems, searchParams]);

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
                                    <td className='row'>
                                        <Col>
                                            <Button 
                                                variant='success mr-2' 
                                                onClick={() => itemSelected(i)}
                                            >
                                                Select
                                            </Button>
                                        </Col>
                                        <Col>
                                            <Button 
                                                variant='danger' 
                                                onClick={() => handleDeleteItem(i.id)}
                                            >
                                                Delete
                                            </Button>
                                        </Col>
                                    </td>
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