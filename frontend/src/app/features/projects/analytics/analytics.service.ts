import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface CompletionRateDto {
  totalTasks: number;
  completedTasks: number;
  ratePercentage: number;
}

export interface HeatmapPointDto {
  date: string; // ISO Date string
  count: number;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private apiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) { }

  getCompletionRate(projectId: string): Observable<CompletionRateDto> {
    return this.http.get<CompletionRateDto>(`${this.apiUrl}/projects/${projectId}/analytics/completion-rate`);
  }

  getActivityHeatmap(projectId: string, year?: number): Observable<HeatmapPointDto[]> {
    let url = `${this.apiUrl}/projects/${projectId}/analytics/activity-heatmap`;
    if (year) {
        url += `?year=${year}`;
    }
    return this.http.get<HeatmapPointDto[]>(url);
  }
}
