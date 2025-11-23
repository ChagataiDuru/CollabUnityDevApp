import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Task, TaskColumn, CreateTaskDto, UpdateTaskDto, MoveTaskDto, CreateColumnDto, ChecklistItem, CreateChecklistItemDto, UpdateChecklistItemDto, TaskComment, CreateCommentDto, TaskTag, AddTagDto, TaskAttachment } from '../models/task.model';

@Injectable({
    providedIn: 'root'
})
/**
 * Service to manage tasks, columns, and task-related entities.
 */
export class TaskService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5000/api';

    /**
     * Retrieves all columns for a project.
     * @param projectId The project ID.
     * @returns An observable of columns.
     */
    getColumns(projectId: string): Observable<TaskColumn[]> {
        return this.http.get<TaskColumn[]>(`${this.apiUrl}/projects/${projectId}/columns`);
    }

    /**
     * Creates a new column for a project.
     * @param projectId The project ID.
     * @param column The column creation data.
     * @returns An observable of the created column.
     */
    createColumn(projectId: string, column: CreateColumnDto): Observable<TaskColumn> {
        return this.http.post<TaskColumn>(`${this.apiUrl}/projects/${projectId}/columns`, column);
    }

    /**
     * Retrieves all tasks for a project.
     * @param projectId The project ID.
     * @returns An observable of tasks.
     */
    getTasks(projectId: string): Observable<Task[]> {
        return this.http.get<Task[]>(`${this.apiUrl}/projects/${projectId}/tasks`);
    }

    /**
     * Creates a new task in a project.
     * @param projectId The project ID.
     * @param task The task creation data.
     * @returns An observable of the created task.
     */
    createTask(projectId: string, task: CreateTaskDto): Observable<Task> {
        return this.http.post<Task>(`${this.apiUrl}/projects/${projectId}/tasks`, task);
    }

    /**
     * Updates an existing task.
     * @param id The task ID.
     * @param task The task update data.
     * @returns An observable of the updated task.
     */
    updateTask(id: string, task: UpdateTaskDto): Observable<Task> {
        return this.http.put<Task>(`${this.apiUrl}/tasks/${id}`, task);
    }

    /**
     * Deletes a task.
     * @param id The task ID.
     * @returns An observable that completes when the deletion is finished.
     */
    deleteTask(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${id}`);
    }

    /**
     * Moves a task to a different column or position.
     * @param id The task ID.
     * @param moveDto The move details.
     * @returns An observable that completes when the move is finished.
     */
    moveTask(id: string, moveDto: MoveTaskDto): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/tasks/${id}/move`, moveDto);
    }

    // Checklist
    /**
     * Adds a checklist item to a task.
     * @param taskId The task ID.
     * @param dto The checklist item creation data.
     * @returns An observable of the created checklist item.
     */
    addChecklistItem(taskId: string, dto: CreateChecklistItemDto): Observable<ChecklistItem> {
        return this.http.post<ChecklistItem>(`${this.apiUrl}/tasks/${taskId}/checklist`, dto);
    }

    /**
     * Updates a checklist item.
     * @param taskId The task ID.
     * @param itemId The checklist item ID.
     * @param dto The checklist item update data.
     * @returns An observable of the updated checklist item.
     */
    updateChecklistItem(taskId: string, itemId: string, dto: UpdateChecklistItemDto): Observable<ChecklistItem> {
        return this.http.put<ChecklistItem>(`${this.apiUrl}/tasks/${taskId}/checklist/${itemId}`, dto);
    }

    /**
     * Deletes a checklist item.
     * @param taskId The task ID.
     * @param itemId The checklist item ID.
     * @returns An observable that completes when the deletion is finished.
     */
    deleteChecklistItem(taskId: string, itemId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}/checklist/${itemId}`);
    }

    // Comments
    /**
     * Adds a comment to a task.
     * @param taskId The task ID.
     * @param dto The comment creation data.
     * @returns An observable of the created comment.
     */
    addComment(taskId: string, dto: CreateCommentDto): Observable<TaskComment> {
        return this.http.post<TaskComment>(`${this.apiUrl}/tasks/${taskId}/comments`, dto);
    }

    /**
     * Deletes a comment from a task.
     * @param taskId The task ID.
     * @param commentId The comment ID.
     * @returns An observable that completes when the deletion is finished.
     */
    deleteComment(taskId: string, commentId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}/comments/${commentId}`);
    }

    // Tags
    /**
     * Adds a tag to a task.
     * @param taskId The task ID.
     * @param dto The tag creation data.
     * @returns An observable of the created tag.
     */
    addTag(taskId: string, dto: AddTagDto): Observable<TaskTag> {
        return this.http.post<TaskTag>(`${this.apiUrl}/tasks/${taskId}/tags`, dto);
    }

    /**
     * Deletes a tag from a task.
     * @param taskId The task ID.
     * @param tagId The tag ID.
     * @returns An observable that completes when the deletion is finished.
     */
    deleteTag(taskId: string, tagId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}/tags/${tagId}`);
    }

    // Attachments
    /**
     * Uploads an attachment to a task.
     * @param taskId The task ID.
     * @param file The file to upload.
     * @returns An observable of the created attachment.
     */
    uploadAttachment(taskId: string, file: File): Observable<TaskAttachment> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<TaskAttachment>(`${this.apiUrl}/tasks/${taskId}/attachments`, formData);
    }

    /**
     * Deletes an attachment from a task.
     * @param taskId The task ID.
     * @param attachmentId The attachment ID.
     * @returns An observable that completes when the deletion is finished.
     */
    deleteAttachment(taskId: string, attachmentId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}/attachments/${attachmentId}`);
    }
}
