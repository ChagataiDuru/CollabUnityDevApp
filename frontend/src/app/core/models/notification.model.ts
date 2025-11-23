export interface Notification {
    id: string;
    title: string;
    message: string;
    type: 'Info' | 'Success' | 'Warning' | 'Error';
    isRead: boolean;
    createdAt: string;
    relatedEntityId?: string;
    relatedEntityType?: string;
}
