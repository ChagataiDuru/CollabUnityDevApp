export interface CreateSprintDto {
    name: string;
    description?: string;
    goal?: string;
    startDate: string; // ISO string
    endDate: string; // ISO string
}
