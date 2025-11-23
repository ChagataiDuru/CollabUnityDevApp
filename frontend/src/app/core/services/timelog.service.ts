import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TimeLog, CreateManualTimeLogDto, StartTimerDto } from '../models/timelog.model';

@Injectable({
    providedIn: 'root'
})
export class TimeLogService {
    private readonly apiUrl = '/api';

    constructor(private http: HttpClient) { }

    getTimeLogs(taskId: string): Observable<TimeLog[]> {
        return this.http.get<TimeLog[]>(`${this.apiUrl}/tasks/${taskId}/timelogs`);
    }

    startTimer(taskId: string, dto: StartTimerDto): Observable<TimeLog> {
        return this.http.post<TimeLog>(`${this.apiUrl}/tasks/${taskId}/timelogs/start`, dto);
    }

    stopTimer(timeLogId: string): Observable<TimeLog> {
        return this.http.put<TimeLog>(`${this.apiUrl}/timelogs/${timeLogId}/stop`, {});
    }

    createManualTimeLog(taskId: string, dto: CreateManualTimeLogDto): Observable<TimeLog> {
        return this.http.post<TimeLog>(`${this.apiUrl}/tasks/${taskId}/timelogs/manual`, dto);
    }

    deleteTimeLog(timeLogId: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/timelogs/${timeLogId}`);
    }

    getTimeReport(projectId: string, startDate?: Date, endDate?: Date): Observable<any> {
        let url = `${this.apiUrl}/projects/${projectId}/timelogs/report`;
        const params: string[] = [];

        if (startDate) {
            params.push(`startDate=${startDate.toISOString()}`);
        }
        if (endDate) {
            params.push(`endDate=${endDate.toISOString()}`);
        }

        if (params.length > 0) {
            url += '?' + params.join('&');
        }

        return this.http.get<any>(url);
    }
}
