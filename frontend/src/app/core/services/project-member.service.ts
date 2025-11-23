import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProjectMember, AddMemberDto, UserDto, ProjectRole } from '../models/project-member.model';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ProjectMemberService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl;

    getProjectMembers(projectId: string): Observable<ProjectMember[]> {
        return this.http.get<ProjectMember[]>(`${this.apiUrl}/projects/${projectId}/members`);
    }

    searchUsers(query: string): Observable<UserDto[]> {
        return this.http.get<UserDto[]>(`${this.apiUrl}/projects/search-users?q=${query}`);
    }

    addMember(projectId: string, dto: AddMemberDto): Observable<ProjectMember> {
        return this.http.post<ProjectMember>(`${this.apiUrl}/projects/${projectId}/members`, dto);
    }

    removeMember(projectId: string, userId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/projects/${projectId}/members/${userId}`);
    }

    updateMemberRole(projectId: string, userId: string, role: ProjectRole): Observable<{ message: string }> {
        return this.http.put<{ message: string }>(`${this.apiUrl}/projects/${projectId}/members/${userId}/role`, role);
    }
}
