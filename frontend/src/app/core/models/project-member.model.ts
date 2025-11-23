export enum ProjectRole {
    Owner = 0,
    Admin = 1,
    Member = 2,
    Viewer = 3
}

export interface ProjectMember {
    id: string;
    userId: string;
    username: string;
    displayName?: string;
    avatarUrl?: string;
    role: ProjectRole;
    joinedAt: Date;
}

export interface UserDto {
    id: string;
    username: string;
    displayName?: string;
    avatarUrl?: string;
}

export interface AddMemberDto {
    userId: string;
    role: ProjectRole;
}
