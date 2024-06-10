'use client'
import { Pagination } from 'flowbite-react'

type Prop = {
    currentPage: number
    pageCount: number
    pageChange: (pageNumber: number) => void
}

export default function AppPagination({ currentPage, pageCount, pageChange }: Prop) {
    return (
        <Pagination
            currentPage={currentPage}
            onPageChange={e => pageChange(e)}
            totalPages={pageCount}
            layout='pagination'
            showIcons={true}
            className='text-blue-500 mb-5'
        />
    )
}
