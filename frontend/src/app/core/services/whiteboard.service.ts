import { Injectable, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

export interface DrawEvent {
    prevX: number;
    prevY: number;
    currX: number;
    currY: number;
    color: string;
    lineWidth: number;
    type: 'draw' | 'clear';
}

@Injectable({
    providedIn: 'root'
})
export class WhiteboardService {
    private authService = inject(AuthService);
    private hubConnection: HubConnection | null = null;

    // Signals for events
    drawEvent = signal<DrawEvent | null>(null);
    clearEvent = signal<boolean>(false);

    constructor() { }

    startConnection(projectId: string) {
        const token = this.authService.getAccessToken();
        if (!token) return;

        this.hubConnection = new HubConnectionBuilder()
            .withUrl('http://localhost:5000/hubs/whiteboard', {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        this.hubConnection.start()
            .then(() => {
                console.log('Whiteboard Hub Connected');
                this.joinWhiteboard(projectId);
            })
            .catch(err => console.error('Error while starting whiteboard connection: ' + err));

        this.registerHandlers();
    }

    private joinWhiteboard(projectId: string) {
        this.hubConnection?.invoke('JoinWhiteboard', projectId)
            .catch(err => console.error(err));
    }

    private registerHandlers() {
        if (!this.hubConnection) return;

        this.hubConnection.on('ReceiveDraw', (data: DrawEvent) => {
            this.drawEvent.set(data);
        });

        this.hubConnection.on('BoardCleared', () => {
            this.clearEvent.set(true);
            // Reset signal immediately so it can trigger again
            setTimeout(() => this.clearEvent.set(false), 0);
        });
    }

    stopConnection() {
        if (this.hubConnection) {
            this.hubConnection.stop();
            this.hubConnection = null;
        }
    }

    sendDraw(projectId: string, event: DrawEvent) {
        this.hubConnection?.invoke('SendDraw', projectId, event)
            .catch(err => console.error(err));
    }

    clearBoard(projectId: string) {
        this.hubConnection?.invoke('ClearBoard', projectId)
            .catch(err => console.error(err));
    }
}
