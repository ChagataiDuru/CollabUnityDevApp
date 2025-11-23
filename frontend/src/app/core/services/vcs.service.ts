import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export enum RepositoryType {
    GitHub = 0,
    GitLab = 1
}

export interface RepositoryDto {
    id: number;
    projectId: string;
    type: RepositoryType;
    url: string;
    webhookSecret?: string;
}

export interface AddRepositoryDto {
    type: RepositoryType;
    url: string;
    webhookSecret?: string;
}

@Injectable({
    providedIn: 'root'
})
export class VcsService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/projects`;

    getRepositories(projectId: string): Observable<RepositoryDto[]> {
        return this.http.get<RepositoryDto[]>(`${this.apiUrl}/${projectId}/integrations/repositories`);
    }

    addRepository(projectId: string, dto: AddRepositoryDto): Observable<RepositoryDto> {
        return this.http.post<RepositoryDto>(`${this.apiUrl}/${projectId}/integrations/repositories`, dto);
    }
}
