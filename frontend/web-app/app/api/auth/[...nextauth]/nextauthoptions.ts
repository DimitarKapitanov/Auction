import { NextAuthOptions } from "next-auth";
import DuendeIdentityServer6 from "next-auth/providers/duende-identity-server6";
const issuerUrl = process.env.NODE_ENV === 'production'
    ? 'http://id.carauction.bg'
    : process.env.ID_URL;

export const authOptions: NextAuthOptions = {
    session: {
        strategy: 'jwt'
    },
    providers: [
        DuendeIdentityServer6({
            id: 'id-server',
            clientId: 'nextApp',
            clientSecret: 'secret',
            issuer: issuerUrl,
            authorization: { params: { scope: 'openid profile auctionApp' } },
            idToken: true
        })
    ],
    callbacks: {
        async jwt({ token, profile, account }) {
            if (profile) {
                token.username = profile.username
            }
            if (account) {
                token.access_token = account.access_token
            }
            return token;
        },
        async session({ session, token }) {
            if (token) {
                session.user.username = token.username
            }
            return session;
        }
    }
}