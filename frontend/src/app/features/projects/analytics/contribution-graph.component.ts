import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TooltipModule } from 'primeng/tooltip';
import { HeatmapPointDto } from './analytics.service';

@Component({
  selector: 'app-contribution-graph',
  standalone: true,
  imports: [CommonModule, TooltipModule],
  template: `
    <div class="flex flex-col gap-2">
      <div class="flex text-xs text-gray-500 gap-1 justify-end items-center mb-1">
        <span>Less</span>
        <div class="w-3 h-3 bg-gray-100 rounded-sm"></div>
        <div class="w-3 h-3 bg-green-200 rounded-sm"></div>
        <div class="w-3 h-3 bg-green-400 rounded-sm"></div>
        <div class="w-3 h-3 bg-green-600 rounded-sm"></div>
        <div class="w-3 h-3 bg-green-800 rounded-sm"></div>
        <span>More</span>
      </div>

      <div class="flex gap-1 overflow-x-auto pb-2">
        <div *ngFor="let week of weeks" class="flex flex-col gap-1">
          <div *ngFor="let day of week"
               class="w-3 h-3 rounded-sm cursor-pointer hover:ring-1 hover:ring-gray-400"
               [ngClass]="getColorClass(day.count)"
               [pTooltip]="getTooltip(day)"
               tooltipPosition="top">
          </div>
        </div>
      </div>
    </div>
  `
})
export class ContributionGraphComponent implements OnChanges {
  @Input() data: HeatmapPointDto[] = [];
  @Input() year: number = new Date().getFullYear();

  weeks: { date: Date, count: number }[][] = [];

  ngOnChanges(changes: SimpleChanges) {
    if (changes['data'] || changes['year']) {
      this.generateCalendar();
    }
  }

  generateCalendar() {
    this.weeks = [];
    const startDate = new Date(this.year, 0, 1);
    const endDate = new Date(this.year, 11, 31);

    // Adjust start date to previous Sunday if not already Sunday
    // Javascript getDay(): 0 = Sunday
    const startDay = startDate.getDay();
    const calendarStart = new Date(startDate);
    calendarStart.setDate(calendarStart.getDate() - startDay);

    const dataMap = new Map<string, number>();
    this.data.forEach(d => {
        dataMap.set(new Date(d.date).toDateString(), d.count);
    });

    let current = new Date(calendarStart);
    let currentWeek: { date: Date, count: number }[] = [];

    // Loop until we pass the end date or complete the last week
    while (current <= endDate || currentWeek.length > 0) {
        if (currentWeek.length === 7) {
            this.weeks.push(currentWeek);
            currentWeek = [];
            if (current > endDate) break;
        }

        // Don't render days from previous/next year if strictly sticking to year?
        // Usually Github graph shows full weeks.

        const count = dataMap.get(current.toDateString()) || 0;
        currentWeek.push({ date: new Date(current), count });

        current.setDate(current.getDate() + 1);
    }
  }

  getColorClass(count: number): string {
    if (count === 0) return 'bg-gray-100';
    if (count <= 2) return 'bg-green-200';
    if (count <= 5) return 'bg-green-400';
    if (count <= 10) return 'bg-green-600';
    return 'bg-green-800';
  }

  getTooltip(day: { date: Date, count: number }): string {
    return `${day.count} contributions on ${day.date.toDateString()}`;
  }
}
