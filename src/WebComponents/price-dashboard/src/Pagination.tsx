import React, { useCallback, useEffect, useState } from "react";

export interface IPaginationProps {
    total: number;
    count: number;
    start: number;
    goTo(index: number): void;
}

function getCurrentPageNumber(start: number, count: number) {
    return Math.floor(start /  count) + 1;
}

const Pagination : React.FC<IPaginationProps> = ({
    total, count, start, goTo
}) => {
    const [pagesNumber, setPagesNumber] = useState(0);

    const btnFirstClick = useCallback(() => {
        goTo(0);
    }, [goTo]);

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

    const btnLastClick = useCallback(() => {
            goTo(count * (pagesNumber - 1));
    }, [count, goTo, pagesNumber]);

    useEffect(() => {
        const pages = Math.ceil(total / count);
        if (pages !== pagesNumber)
            setPagesNumber(pages);
    }, [total, count]);

    return <nav>
        <ul className="pagination">
            <li className="page-item">
                <a className="page-link" onClick={btnFirstClick}>
                    <span>First</span>
                </a>
            </li>
            <li className="page-item">
                <a className="page-link" onClick={btnPreviousClick}>
                    <span>&laquo;</span>
                </a>
            </li>
            <li className="page-item active">
                <span className="page-link">
                    {getCurrentPageNumber(start, count)}
                </span>
            </li>
            <li className="page-item">
                <a className="page-link" onClick={btnNextClick}>
                    <span>&raquo;</span>
                </a>
            </li>
            <li className="page-item">
                <a className="page-link" onClick={btnLastClick}>
                    <span>Last</span>
                </a>
            </li>
        </ul>
    </nav>
}

export default Pagination;