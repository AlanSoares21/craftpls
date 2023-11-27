import React, { useCallback, useEffect, useState } from "react";
import { Pagination } from "react-bootstrap";

export interface IStandardPaginationProps {
    total: number;
    count: number;
    start: number;
    goTo(index: number): void;
}

function getCurrentPageNumber(start: number, count: number) {
    return Math.floor(start /  count) + 1;
}

const StandardPagination : React.FC<IStandardPaginationProps> = ({
    total, count, start, goTo
}) => {
    const [pagesNumber, setPagesNumber] = useState(0);

    const btnPreviousClick = useCallback(() => {
        const page = getCurrentPageNumber(start, count);
        if (page > 1)
            goTo(start - count);
    }, [start, count, goTo]);

    const btnNextClick = useCallback(() => {
        const page = getCurrentPageNumber(start, count);
        if (page < pagesNumber)
            goTo(start + count);
    }, [start, count, goTo, pagesNumber]);

    useEffect(() => {
        const pages = Math.ceil(total / count);
        if (pages !== pagesNumber)
            setPagesNumber(pages);
    }, [total, count]);

    return <Pagination>
        <Pagination.Prev onClick={btnPreviousClick} />
        <Pagination.Item disabled>
            {getCurrentPageNumber(start, count)}
        </Pagination.Item>
        <Pagination.Next onClick={btnNextClick} />
    </Pagination>
}

export default StandardPagination;