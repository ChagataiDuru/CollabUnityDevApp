import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    private authService = inject(AuthService);
    public hubConnection: HubConnection | null = null;
    private projectEventsSubject = new BehaviorSubject<any>(null);

    projectEvents$ = this.projectEventsSubject.asObservable();

    constructor() { }

    startConnection(projectId: string) {
        const token = this.authService.getAccessToken();
        if (!token) return;

        this.hubConnection = new HubConnectionBuilder()
            .withUrl('http://localhost:5000/hubs/project', {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        this.hubConnection.start()
            .then(() => {
                console.log('SignalR Connected');
                this.joinProject(projectId);
            })
            .catch(err => console.error('Error while starting connection: ' + err));

        this.registerHandlers();
    }

    private joinProject(projectId: string) {
        this.hubConnection?.invoke('JoinProject', projectId)
            .catch(err => console.error(err));
    }

    private registerHandlers() {
        if (!this.hubConnection) return;

        const events = [
            'TaskCreated', 'TaskUpdated', 'TaskDeleted', 'TaskMoved',
            'ColumnCreated', 'ColumnUpdated', 'ColumnDeleted', 'ColumnsReordered'
        ];

        events.forEach(event => {
            this.hubConnection!.on(event, (data: any, ...args: any[]) => {
                this.projectEventsSubject.next({ type: event, data, args });
            });
        });
    }

    stopConnection() {
        if (this.hubConnection) {
            this.hubConnection.stop();
            this.hubConnection = null;
        }
    }
}
