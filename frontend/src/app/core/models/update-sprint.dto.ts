import { SprintStatus } from './sprint.model';

export interface UpdateSprintDto {
    name: string;
    description?: string;
    goal?: string;
    startDate: string; // ISO string
    endDate: string; // ISO string
    status: SprintStatus;
}
