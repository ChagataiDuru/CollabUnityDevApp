import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { User, TokenResponse } from '../models/user.model';
import { Router } from '@angular/router';

@Injectable({
    providedIn: 'root'
})
/**
 * Service to handle user authentication and session management.
 */
export class AuthService {
    private apiUrl = 'http://localhost:5000/api/auth';
    private currentUserSubject = new BehaviorSubject<User | null>(null);

    currentUser$ = this.currentUserSubject.asObservable();
    isAuthenticated = signal<boolean>(false);

    constructor(private http: HttpClient, private router: Router) {
        this.loadUserFromStorage();
    }

    /**
     * Loads the user session from local storage.
     */
    private loadUserFromStorage() {
        const token = localStorage.getItem('accessToken');
        if (token) {
            // In a real app, we'd decode the token or fetch /me
            // For MVP, we'll just assume logged in if token exists and maybe decode payload
            this.isAuthenticated.set(true);
            // Decode token to get user info (simplified)
            const payload = JSON.parse(atob(token.split('.')[1]));
            this.currentUserSubject.next({
                id: payload.nameid,
                username: payload.unique_name,

            });
        }
    }

    /**
     * Registers a new user.
     * @param data The registration data.
     * @returns An observable of the token response.
     */
    register(data: any): Observable<TokenResponse> {
        return this.http.post<TokenResponse>(`${this.apiUrl}/register`, data).pipe(
            tap(response => this.handleAuthResponse(response))
        );
    }

    /**
     * Logs in a user.
     * @param data The login credentials.
     * @returns An observable of the token response.
     */
    login(data: any): Observable<TokenResponse> {
        return this.http.post<TokenResponse>(`${this.apiUrl}/login`, data).pipe(
            tap(response => this.handleAuthResponse(response))
        );
    }

    /**
     * Logs out the current user and clears the session.
     */
    logout() {
        const refreshToken = localStorage.getItem('refreshToken');
        if (refreshToken) {
            this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe();
        }
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        this.currentUserSubject.next(null);
        this.isAuthenticated.set(false);
        this.router.navigate(['/login']);
    }

    /**
     * Handles the authentication response by saving tokens and updating state.
     * @param response The token response from the server.
     */
    private handleAuthResponse(response: TokenResponse) {
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
        this.isAuthenticated.set(true);

        const payload = JSON.parse(atob(response.accessToken.split('.')[1]));
        this.currentUserSubject.next({
            id: payload.nameid,
            username: payload.unique_name,

        });
    }

    /**
     * Retrieves the current access token.
     * @returns The access token or null if not found.
     */
    getAccessToken(): string | null {
        return localStorage.getItem('accessToken');
    }
}
