export interface Sprint {
    id: string;
    projectId: string;
    name: string;
    description?: string;
    goal?: string;
    startDate: string; // ISO string
    endDate: string; // ISO string
    status: SprintStatus;
    createdAt: string;
    updatedAt: string;
    taskCount: number;
    completedTaskCount: number;
}

export enum SprintStatus {
    Planned = 'Planned',
    Active = 'Active',
    Completed = 'Completed',
    Archived = 'Archived'
}
