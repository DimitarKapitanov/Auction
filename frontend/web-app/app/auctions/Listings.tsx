'use client'

import qs from 'query-string';
import { useEffect, useState } from "react";
import { shallow } from 'zustand/shallow';
import { getData } from '../actions/auctionActions';
import AppPagination from "../components/AppPagination";
import EmptyFilter from '../components/EmptyFilter';
import { useParamsStore } from "../hooks/useParamStore";
import { Auction, PageResult } from "../types";
import AuctionCard from "./AuctionCard";
import Filters from "./Filters";

export default function Listings() {
    const [data, setData] = useState<PageResult<Auction>>();
    const params = useParamsStore(state => ({
        pageNumber: state.pageNumber,
        pageSize: state.pageSize,
        searchTerm: state.searchTerm,
        orderBy: state.orderBy,
        filterBy: state.filterBy
    }), shallow);
    const setParams = useParamsStore(state => state.setParams);
    const url = qs.stringifyUrl({ url: '', query: params });

    function setPageNumber(pageNumber: number) {
        setParams({ pageNumber });
    }

    useEffect(() => {
        getData(url).then(data => {
            setData(data);
        })
    }, [url]);

    if (!data) return <h3>Loading...</h3>

    return (
        <>
            <Filters />
            {data.totalCount === 0 ? (
                <EmptyFilter showReset title="No matches for this filter" subTitle="Try changing or resetting the filter" />
            ) : (
                <>
                    <div className="grid grid-cols-4 gap-6">
                        {data.results.map(auction => (
                            <AuctionCard key={auction.id} auction={auction} />
                        ))}
                    </div>
                    <div className="flex justify-center mt-4">
                        <AppPagination currentPage={params.pageNumber} pageCount={data.pageCount} pageChanged={setPageNumber} />
                    </div>
                </>
            )}

        </>
    )
}

