import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Notification } from '../models/notification.model';
import { tap } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/notifications`;

    // Signals for state management
    notifications = signal<Notification[]>([]);
    unreadCount = signal<number>(0);

    constructor() {
        this.loadNotifications();
    }

    loadNotifications() {
        this.http.get<Notification[]>(this.apiUrl).subscribe(notifications => {
            this.notifications.set(notifications);
            this.updateUnreadCount();
        });
    }

    markAsRead(id: string) {
        return this.http.put(`${this.apiUrl}/${id}/read`, {}).pipe(
            tap(() => {
                this.notifications.update(current =>
                    current.map(n => n.id === id ? { ...n, isRead: true } : n)
                );
                this.updateUnreadCount();
            })
        );
    }

    markAllAsRead() {
        return this.http.put(`${this.apiUrl}/read-all`, {}).pipe(
            tap(() => {
                this.notifications.update(current =>
                    current.map(n => ({ ...n, isRead: true }))
                );
                this.updateUnreadCount();
            })
        );
    }

    deleteNotification(id: string) {
        return this.http.delete(`${this.apiUrl}/${id}`).pipe(
            tap(() => {
                this.notifications.update(current =>
                    current.filter(n => n.id !== id)
                );
                this.updateUnreadCount();
            })
        );
    }

    // Called when a real-time notification is received via SignalR
    addNotification(notification: Notification) {
        this.notifications.update(current => [notification, ...current]);
        this.updateUnreadCount();
    }

    private updateUnreadCount() {
        this.unreadCount.set(this.notifications().filter(n => !n.isRead).length);
    }
}
