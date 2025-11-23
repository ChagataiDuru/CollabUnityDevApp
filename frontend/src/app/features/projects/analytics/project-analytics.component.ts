import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AnalyticsService, CompletionRateDto, HeatmapPointDto } from './analytics.service';
import { ContributionGraphComponent } from './contribution-graph.component';
import { CardModule } from 'primeng/card';
import { KnobModule } from 'primeng/knob';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-project-analytics',
  standalone: true,
  imports: [CommonModule, ContributionGraphComponent, CardModule, KnobModule, FormsModule],
  template: `
    <div class="p-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <!-- Completion Rate -->
      <p-card header="Completion Rate" styleClass="h-full">
        <div class="flex flex-col items-center justify-center py-4">
            <p-knob [(ngModel)]="completionRate" [readonly]="true" [valueColor]="'#10b981'" [size]="150" valueTemplate="{value}%"></p-knob>
            <div class="mt-4 text-center text-gray-600">
                <p><span class="font-bold text-gray-800">{{ completedTasks }}</span> closed tasks</p>
                <p>out of <span class="font-bold text-gray-800">{{ totalTasks }}</span> total</p>
            </div>
        </div>
      </p-card>

      <!-- Activity Heatmap -->
      <p-card header="Activity Heatmap" styleClass="h-full col-span-1 md:col-span-2 lg:col-span-2">
         <div class="overflow-x-auto">
            <app-contribution-graph [data]="heatmapData"></app-contribution-graph>
         </div>
      </p-card>
    </div>
  `
})
export class ProjectAnalyticsComponent implements OnInit {
  projectId: string = '';

  completionRate: number = 0;
  completedTasks: number = 0;
  totalTasks: number = 0;

  heatmapData: HeatmapPointDto[] = [];

  constructor(
    private route: ActivatedRoute,
    private analyticsService: AnalyticsService
  ) {}

  ngOnInit() {
    this.route.parent?.params.subscribe(params => {
        this.projectId = params['id'];
        if (this.projectId) {
            this.loadData();
        }
    });
  }

  loadData() {
    this.analyticsService.getCompletionRate(this.projectId).subscribe(data => {
        this.completionRate = Math.round(data.ratePercentage);
        this.completedTasks = data.completedTasks;
        this.totalTasks = data.totalTasks;
    });

    this.analyticsService.getActivityHeatmap(this.projectId).subscribe(data => {
        this.heatmapData = data;
    });
  }
}
