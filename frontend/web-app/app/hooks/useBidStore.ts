import { createWithEqualityFn } from "zustand/traditional"
import { Bid } from "../types"

type State = {
    bids: Bid[]
}
type Actions = {
    setBids: (bids: Bid[]) => void
    addBid: (bid: Bid) => void
}

export const useBidStore = createWithEqualityFn<State & Actions>((set) => ({
    bids: [],
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
}))