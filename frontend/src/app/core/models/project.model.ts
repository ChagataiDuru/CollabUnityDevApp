export interface Project {
    id: string;
    name: string;
    description?: string;
    colorTheme: string;
    createdById?: string;
    createdByName?: string;
    createdAt: string;
    updatedAt: string;
}

export interface CreateProjectDto {
    name: string;
    description?: string;
    colorTheme?: string;
}
