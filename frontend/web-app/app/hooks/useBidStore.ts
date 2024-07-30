import { createWithEqualityFn } from "zustand/traditional"
import { Bid } from "../types"

type State = {
    bids: Bid[]
    open: boolean
}
type Actions = {
    setBids: (bids: Bid[]) => void
    addBid: (bid: Bid) => void
    setOpen: (value: boolean) => void
}

export const useBidStore = createWithEqualityFn<State & Actions>((set) => ({
    bids: [],
    open: true,
    setBids: (bids) => {
        set(() => ({
            bids: bids
        }))
    },
    addBid: (bid) => {
        set((state) => ({
            bids: !state.bids.find(x => x.id === bid.id) ? [bid, ...state.bids] : [...state.bids]
        }))
    },
    setOpen: (value) => {
        set(() => ({
            open: value
        }))
    }
}))