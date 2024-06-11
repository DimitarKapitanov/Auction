import { Button } from "flowbite-react"
import { useParamsStore } from "../hooks/useParamStore"
import Heading from "./Heading"

type Props = {
    title?: string
    subTitle?: string
    showReset?: boolean
}

export default function EmptyFilter({ title = 'No matches for this filter', subTitle = 'Try changing or resetting the filter', showReset }: Props) {
    const reset = useParamsStore((state) => state.reset)

    return (
        <div className="h-[40vh] flex flex-col gap-2 justify-center items-center shadow-lg">
            <Heading title={title} subTitle={subTitle} center />
            <div className="mt-4">
                {showReset && (
                    <Button outline onClick={reset}>
                        Reset Filter
                    </Button>
                )}
            </div>
        </div>
    )
}
