import { numberWithCommas } from '@/app/lib/numberWithComma'
import { Bid } from '@/app/types'
import { format } from 'date-fns'
type Props = {
    bid: Bid
}

export default function BidItem({ bid }: Props) {
    function getBidInfo() {
        let bgColor = ''
        let text = ''
        switch (bid.bidStatus) {
            case 'Accepted':
                bgColor = 'bg-green-200'
                text = 'Bid accepted'
                break;
            case 'AcceptedBelowReserve':
                bgColor = 'bg-amber-500'
                text = 'Reserve not met'
                break;
            case 'TooLow':
                bgColor = 'bg-red-200'
                text = 'Bid too low'
                break;
            default:
                bgColor = 'bg-red-200'
                text = 'Bid placed after auction finished'
                break;
        }

        return { bgColor, text }
    }

    return (
        <div className={`
            border-gray-300 border-2 px-3 py-2 flex 
            rounded-lg justify-between items-center mb-2
            ${getBidInfo().bgColor}
        `}>
            <div className='flex flex-col'>
                <span>Bidder: {bid.bidder}</span>
                <span className="text-sm text-gray-700">Time: {format(new Date(bid.bidTime), "dd MMM yyyy h:mm a")}</span>
            </div>
            <div className="flex flex-col text-right">
                <div className='text-xl font-semibold'>
                    ${numberWithCommas(bid.amount)}
                </div>
                <div className='flex flex-row items-center'>
                    <span>{getBidInfo().text}</span>
                </div>
            </div>
        </div>
    )
}