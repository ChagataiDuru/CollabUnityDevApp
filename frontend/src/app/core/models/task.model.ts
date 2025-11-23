export interface Task {
    id: string;
    projectId: string;
    columnId: string;
    title: string;
    description?: string;
    assignedToId?: string;
    assignedToName?: string;
    assignedToAvatar?: string;
    priority: number;
    dueDate?: string;
    estimatedHours?: number;
    position: number;
    createdAt: string;
    updatedAt: string;
    commentsCount: number;
    attachmentsCount: number;
    checklistTotal: number;
    checklistCompleted: number;
    tags: TaskTag[];
    checklistItems?: ChecklistItem[];
    comments?: TaskComment[];
    attachments?: TaskAttachment[];
    commits?: Commit[];
}

export interface Commit {
    id: number;
    hash: string;
    message: string;
    authorName: string;
    timestamp: string;
    url: string;
}

export interface TaskTag {
    id: string;
    name: string;
    color: string;
}

export interface ChecklistItem {
    id: string;
    title: string;
    isCompleted: boolean;
}

export interface TaskComment {
    id: string;
    content: string;
    createdAt: string;
    userId: string;
    userName: string;
    userAvatar?: string;
}

export interface TaskAttachment {
    id: string;
    fileName: string;
    filePath: string;
    uploadedAt: string;
    uploadedById: string;
    uploadedByName: string;
}

export interface TaskColumn {
    id: string;
    projectId: string;
    name: string;
    position: number;
    color: string;
}

export interface CreateTaskDto {
    columnId: string;
    title: string;
    description?: string;
    assignedToId?: string;
    priority?: number;
    dueDate?: string;
    estimatedHours?: number;
}

export interface UpdateTaskDto {
    title: string;
    description?: string;
    assignedToId?: string;
    priority?: number;
    dueDate?: string;
    estimatedHours?: number;
}

export interface MoveTaskDto {
    newColumnId: string;
    newPosition: number;
}

export interface CreateColumnDto {
    name: string;
    color?: string;
}

export interface CreateChecklistItemDto {
    title: string;
}

export interface UpdateChecklistItemDto {
    title: string;
    isCompleted: boolean;
}

export interface CreateCommentDto {
    content: string;
}

export interface AddTagDto {
    name: string;
    color?: string;
}
