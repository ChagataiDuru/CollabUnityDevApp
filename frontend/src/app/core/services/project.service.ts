import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Project, CreateProjectDto } from '../models/project.model';

@Injectable({
    providedIn: 'root'
})
export class ProjectService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5000/api/projects';

    getProjects(): Observable<Project[]> {
        return this.http.get<Project[]>(this.apiUrl);
    }

    getProject(id: string): Observable<Project> {
        return this.http.get<Project>(`${this.apiUrl}/${id}`);
    }

    createProject(project: CreateProjectDto): Observable<Project> {
        return this.http.post<Project>(this.apiUrl, project);
    }

    updateProject(id: string, project: CreateProjectDto): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, project);
    }

    deleteProject(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
