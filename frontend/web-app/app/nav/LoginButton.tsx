'use client'

import { Button } from 'flowbite-react'
import { signIn } from 'next-auth/react'

export default function LoginButton() {
    return (
        <Button onClick={() => signIn('id-server', { callbackUrl: '/' })}>
            Login
        </Button>
    )
}
