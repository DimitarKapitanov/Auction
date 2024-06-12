
type Props = {
    title: string
    subTitle?: string
    center?: boolean
}

export default function Heading({ title, center, subTitle }: Props) {
    return (
        <div className={center ? 'text-center' : 'text-start'}>
            <div className="text-2xl font-bold">
                {title}
            </div>
            <div className="font-light text-neutral-500">
                {subTitle}
            </div>
        </div>
    )
}
