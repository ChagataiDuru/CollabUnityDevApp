import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';
import { DocumentationService } from '../../core/services/documentation.service';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { MenuItem } from 'primeng/api';

@Component({
    selector: 'app-navbar',
    standalone: true,
    imports: [
        CommonModule, RouterLink, RouterLinkActive,
        ButtonModule, MenuModule, AvatarModule, BadgeModule, OverlayPanelModule
    ],
    templateUrl: './navbar.component.html',
    styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
    authService = inject(AuthService);
    notificationService = inject(NotificationService);
    themeService = inject(ThemeService);
    docService = inject(DocumentationService);

    currentUser = toSignal(this.authService.currentUser$);
    notifications = this.notificationService.notifications;
    unreadCount = this.notificationService.unreadCount;

    userMenuItems: MenuItem[] = [
        {
            label: 'Profile',
            icon: 'pi pi-user',
            command: () => { /* TODO: Navigate to profile */ }
        },
        {
            separator: true
        },
        {
            label: 'Logout',
            icon: 'pi pi-sign-out',
            command: () => this.authService.logout()
        }
    ];

    toggleTheme() {
        this.themeService.toggleTheme();
    }

    markAsRead(id: string, event: Event) {
        event.stopPropagation();
        this.notificationService.markAsRead(id).subscribe();
    }

    markAllRead() {
        this.notificationService.markAllAsRead().subscribe();
    }

    deleteNotification(id: string, event: Event) {
        event.stopPropagation();
        this.notificationService.deleteNotification(id).subscribe();
    }

    getNotificationIcon(type: string): string {
        switch (type) {
            case 'Success': return 'pi pi-check-circle text-green-500';
            case 'Warning': return 'pi pi-exclamation-triangle text-orange-500';
            case 'Error': return 'pi pi-times-circle text-red-500';
            default: return 'pi pi-info-circle text-blue-500';
        }
    }
}
