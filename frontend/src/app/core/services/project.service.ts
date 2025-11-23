import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Project, CreateProjectDto } from '../models/project.model';

@Injectable({
    providedIn: 'root'
})
/**
 * Service to manage project-related operations.
 */
export class ProjectService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5000/api/projects';

    currentProject = signal<Project | null>(null);

    /**
     * Retrieves all projects for the current user.
     * @returns An observable containing a list of projects.
     */
    getProjects(): Observable<Project[]> {
        return this.http.get<Project[]>(this.apiUrl);
    }

    /**
     * Retrieves a specific project by ID.
     * @param id The project ID.
     * @returns An observable containing the project details.
     */
    getProject(id: string): Observable<Project> {
        return this.http.get<Project>(`${this.apiUrl}/${id}`).pipe(
            tap(project => this.currentProject.set(project))
        );
    }

    /**
     * Creates a new project.
     * @param project The project creation data.
     * @returns An observable containing the created project.
     */
    createProject(project: CreateProjectDto): Observable<Project> {
        return this.http.post<Project>(this.apiUrl, project);
    }

    /**
     * Updates an existing project.
     * @param id The project ID.
     * @param project The project update data.
     * @returns An observable that completes when the update is finished.
     */
    updateProject(id: string, project: CreateProjectDto): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, project);
    }

    /**
     * Deletes a project.
     * @param id The project ID.
     * @returns An observable that completes when the deletion is finished.
     */
    deleteProject(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
