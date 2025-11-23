import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Task, TaskColumn, CreateTaskDto, UpdateTaskDto, MoveTaskDto, CreateColumnDto, ChecklistItem, CreateChecklistItemDto, UpdateChecklistItemDto, TaskComment, CreateCommentDto, TaskTag, AddTagDto, TaskAttachment } from '../models/task.model';

@Injectable({
    providedIn: 'root'
})
export class TaskService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5000/api';

    getColumns(projectId: string): Observable<TaskColumn[]> {
        return this.http.get<TaskColumn[]>(`${this.apiUrl}/projects/${projectId}/columns`);
    }

    createColumn(projectId: string, column: CreateColumnDto): Observable<TaskColumn> {
        return this.http.post<TaskColumn>(`${this.apiUrl}/projects/${projectId}/columns`, column);
    }

    getTasks(projectId: string): Observable<Task[]> {
        return this.http.get<Task[]>(`${this.apiUrl}/projects/${projectId}/tasks`);
    }

    createTask(projectId: string, task: CreateTaskDto): Observable<Task> {
        return this.http.post<Task>(`${this.apiUrl}/projects/${projectId}/tasks`, task);
    }

    updateTask(id: string, task: UpdateTaskDto): Observable<Task> {
        return this.http.put<Task>(`${this.apiUrl}/tasks/${id}`, task);
    }

    deleteTask(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${id}`);
    }

    moveTask(id: string, moveDto: MoveTaskDto): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/tasks/${id}/move`, moveDto);
    }

    // Checklist
    addChecklistItem(taskId: string, dto: CreateChecklistItemDto): Observable<ChecklistItem> {
        return this.http.post<ChecklistItem>(`${this.apiUrl}/tasks/${taskId}/checklist`, dto);
    }

    updateChecklistItem(taskId: string, itemId: string, dto: UpdateChecklistItemDto): Observable<ChecklistItem> {
        return this.http.put<ChecklistItem>(`${this.apiUrl}/tasks/${taskId}/checklist/${itemId}`, dto);
    }

    deleteChecklistItem(taskId: string, itemId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}/checklist/${itemId}`);
    }

    // Comments
    addComment(taskId: string, dto: CreateCommentDto): Observable<TaskComment> {
        return this.http.post<TaskComment>(`${this.apiUrl}/tasks/${taskId}/comments`, dto);
    }

    deleteComment(taskId: string, commentId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}/comments/${commentId}`);
    }

    // Tags
    addTag(taskId: string, dto: AddTagDto): Observable<TaskTag> {
        return this.http.post<TaskTag>(`${this.apiUrl}/tasks/${taskId}/tags`, dto);
    }

    deleteTag(taskId: string, tagId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}/tags/${tagId}`);
    }

    // Attachments
    uploadAttachment(taskId: string, file: File): Observable<TaskAttachment> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<TaskAttachment>(`${this.apiUrl}/tasks/${taskId}/attachments`, formData);
    }

    deleteAttachment(taskId: string, attachmentId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}/attachments/${attachmentId}`);
    }
}
