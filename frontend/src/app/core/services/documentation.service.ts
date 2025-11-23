import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SearchResult {
    title: string;
    link: string;
    snippet: string;
}

export interface PinnedDoc {
    id: string;
    projectId: string;
    title: string;
    url: string;
    description?: string;
    notes?: string;
    savedById?: string;
    savedByName?: string;
    createdAt: Date;
}

export interface CreatePinnedDocDto {
    title: string;
    url: string;
    description?: string;
    notes?: string;
}

export interface SearchHistory {
    id: string;
    query: string;
    searchedAt: Date;
}

@Injectable({
    providedIn: 'root'
})
export class DocumentationService {
    private readonly apiUrl = '/api';

    // Use a signal for reactive state
    panelVisible = signal<boolean>(false);

    togglePanel() {
        this.panelVisible.update(v => !v);
    }

    constructor(private http: HttpClient) { }

    search(query: string): Observable<SearchResult[]> {
        return this.http.get<SearchResult[]>(`${this.apiUrl}/documentation/search`, {
            params: { query }
        });
    }

    getPinnedDocs(projectId: string): Observable<PinnedDoc[]> {
        return this.http.get<PinnedDoc[]>(`${this.apiUrl}/projects/${projectId}/documentation/pinned`);
    }

    pinDoc(projectId: string, dto: CreatePinnedDocDto): Observable<PinnedDoc> {
        return this.http.post<PinnedDoc>(`${this.apiUrl}/projects/${projectId}/documentation/pinned`, dto);
    }

    unpinDoc(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/documentation/pinned/${id}`);
    }

    getHistory(): Observable<SearchHistory[]> {
        return this.http.get<SearchHistory[]>(`${this.apiUrl}/documentation/history`);
    }
}
