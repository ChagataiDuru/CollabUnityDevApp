export interface User {
    id: string;
    username: string;

    displayName?: string;
    avatarUrl?: string;
}

export interface TokenResponse {
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
}
