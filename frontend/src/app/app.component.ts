import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SignalRService } from './core/services/signalr.service';
import { NotificationService } from './core/services/notification.service';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastModule],
  providers: [MessageService],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'unity-dev-hub';

  private signalRService = inject(SignalRService);
  private notificationService = inject(NotificationService);
  private messageService = inject(MessageService);

  ngOnInit() {
    // Listen for global notifications
    if (this.signalRService.hubConnection) {
      this.signalRService.hubConnection.on('NotificationReceived', (notification) => {
        this.notificationService.addNotification(notification);
        this.messageService.add({
          severity: notification.type.toLowerCase(),
          summary: notification.title,
          detail: notification.message,
          life: 5000
        });
      });
    }
  }

  ngOnDestroy() {
    if (this.signalRService.hubConnection) {
      this.signalRService.hubConnection.off('NotificationReceived');
    }
  }
}
