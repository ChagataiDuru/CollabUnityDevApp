import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Sprint } from '../models/sprint.model';
import { CreateSprintDto } from '../models/create-sprint.dto';
import { UpdateSprintDto } from '../models/update-sprint.dto';

@Injectable({
    providedIn: 'root'
})
export class SprintService {
    private readonly apiUrl = '/api';

    constructor(private http: HttpClient) { }

    getSprints(projectId: string): Observable<Sprint[]> {
        return this.http.get<Sprint[]>(`${this.apiUrl}/projects/${projectId}/sprints`);
    }

    getSprint(id: string): Observable<Sprint> {
        return this.http.get<Sprint>(`${this.apiUrl}/sprints/${id}`);
    }

    createSprint(projectId: string, dto: CreateSprintDto): Observable<Sprint> {
        return this.http.post<Sprint>(`${this.apiUrl}/projects/${projectId}/sprints`, dto);
    }

    updateSprint(id: string, dto: UpdateSprintDto): Observable<Sprint> {
        return this.http.put<Sprint>(`${this.apiUrl}/sprints/${id}`, dto);
    }

    deleteSprint(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/sprints/${id}`);
    }

    getBurndownData(id: string): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/sprints/${id}/burndown`);
    }
}
