export interface TimeLog {
    id: string;
    taskId: string;
    userId: string;
    userName: string;
    startTime?: string; // ISO string
    endTime?: string; // ISO string
    durationMinutes: number;
    description?: string;
    isManual: boolean;
    createdAt: string;
}

export interface CreateManualTimeLogDto {
    startTime: string; // ISO string
    endTime: string; // ISO string
    description?: string;
}

export interface StartTimerDto {
    description?: string;
}
