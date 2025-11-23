import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { ProgressBarModule } from 'primeng/progressbar';
import { TagModule } from 'primeng/tag';
import { SprintService } from '../../../core/services/sprint.service';
import { Sprint, SprintStatus } from '../../../core/models/sprint.model';
import { CreateSprintDto } from '../../../core/models/create-sprint.dto';

@Component({
  selector: 'app-sprint-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CardModule,
    DialogModule,
    InputTextModule,
    InputTextareaModule,
    CalendarModule,
    DropdownModule,
    ProgressBarModule,
    TagModule
  ],
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-white">Sprints</h1>
        <p-button 
          label="Create Sprint" 
          icon="pi pi-plus" 
          (onClick)="showCreateDialog = true"
          styleClass="p-button-success">
        </p-button>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <p-card *ngFor="let sprint of sprints" 
                class="cursor-pointer hover:shadow-lg transition-shadow"
                (click)="viewSprint(sprint.id)">
          <ng-template pTemplate="header">
            <div class="p-4 flex justify-between items-start">
              <div>
                <h3 class="text-xl font-semibold text-white">{{ sprint.name }}</h3>
                <p class="text-sm text-gray-400">{{ formatDateRange(sprint.startDate, sprint.endDate) }}</p>
              </div>
              <p-tag [value]="sprint.status" [severity]="getStatusSeverity(sprint.status)"></p-tag>
            </div>
          </ng-template>
          
          <div class="space-y-3">
            <p *ngIf="sprint.goal" class="text-gray-300 text-sm">{{ sprint.goal }}</p>
            
            <div>
              <div class="flex justify-between text-sm mb-1">
                <span class="text-gray-400">Progress</span>
                <span class="text-white">{{ sprint.completedTaskCount }} / {{ sprint.taskCount }} tasks</span>
              </div>
              <p-progressBar 
                [value]="getProgress(sprint)" 
                [showValue]="false"
                styleClass="h-2">
              </p-progressBar>
            </div>
          </div>
        </p-card>

        <div *ngIf="sprints.length === 0" class="col-span-full text-center py-12">
          <i class="pi pi-calendar text-6xl text-gray-600 mb-4"></i>
          <p class="text-gray-400 text-lg">No sprints yet. Create your first sprint to get started!</p>
        </div>
      </div>

      <!-- Create Sprint Dialog -->
      <p-dialog 
        header="Create Sprint" 
        [(visible)]="showCreateDialog" 
        [modal]="true"
        [style]="{width: '500px'}"
        [draggable]="false"
        [resizable]="false">
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-2">Name *</label>
            <input 
              type="text" 
              pInputText 
              [(ngModel)]="newSprint.name"
              class="w-full"
              placeholder="Sprint 1" />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-300 mb-2">Goal</label>
            <textarea 
              pInputTextarea 
              [(ngModel)]="newSprint.goal"
              class="w-full"
              rows="2"
              placeholder="What do you want to achieve in this sprint?">
            </textarea>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-300 mb-2">Description</label>
            <textarea 
              pInputTextarea 
              [(ngModel)]="newSprint.description"
              class="w-full"
              rows="3"
              placeholder="Additional details...">
            </textarea>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-300 mb-2">Start Date *</label>
              <p-calendar 
                [(ngModel)]="startDate"
                [showIcon]="true"
                dateFormat="yy-mm-dd"
                class="w-full">
              </p-calendar>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-300 mb-2">End Date *</label>
              <p-calendar 
                [(ngModel)]="endDate"
                [showIcon]="true"
                dateFormat="yy-mm-dd"
                [minDate]="startDate"
                class="w-full">
              </p-calendar>
            </div>
          </div>
        </div>

        <ng-template pTemplate="footer">
          <p-button 
            label="Cancel" 
            icon="pi pi-times" 
            (onClick)="showCreateDialog = false"
            styleClass="p-button-text">
          </p-button>
          <p-button 
            label="Create" 
            icon="pi pi-check" 
            (onClick)="createSprint()"
            [disabled]="!isFormValid()"
            styleClass="p-button-success">
          </p-button>
        </ng-template>
      </p-dialog>
    </div>
  `,
  styles: ``
})
export class SprintListComponent implements OnInit {
  sprints: Sprint[] = [];
  projectId: string = '';
  showCreateDialog = false;

  newSprint: Partial<CreateSprintDto> = {
    name: '',
    goal: '',
    description: ''
  };

  startDate: Date | null = null;
  endDate: Date | null = null;

  constructor(
    private sprintService: SprintService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.projectId = params['projectId'];
      this.loadSprints();
    });
  }

  loadSprints() {
    this.sprintService.getSprints(this.projectId).subscribe({
      next: (sprints: Sprint[]) => {
        this.sprints = sprints;
      },
      error: (error: any) => {
        console.error('Error loading sprints:', error);
      }
    });
  }

  createSprint() {
    if (!this.isFormValid()) return;

    const dto: CreateSprintDto = {
      name: this.newSprint.name!,
      goal: this.newSprint.goal,
      description: this.newSprint.description,
      startDate: this.startDate!.toISOString(),
      endDate: this.endDate!.toISOString()
    };

    this.sprintService.createSprint(this.projectId, dto).subscribe({
      next: () => {
        this.showCreateDialog = false;
        this.resetForm();
        this.loadSprints();
      },
      error: (error: any) => {
        console.error('Error creating sprint:', error);
      }
    });
  }

  viewSprint(sprintId: string) {
    this.router.navigate(['/projects', this.projectId, 'sprints', sprintId]);
  }

  isFormValid(): boolean {
    return !!(this.newSprint.name && this.startDate && this.endDate);
  }

  resetForm() {
    this.newSprint = {
      name: '',
      goal: '',
      description: ''
    };
    this.startDate = null;
    this.endDate = null;
  }

  getProgress(sprint: Sprint): number {
    if (sprint.taskCount === 0) return 0;
    return Math.round((sprint.completedTaskCount / sprint.taskCount) * 100);
  }

  getStatusSeverity(status: SprintStatus): 'success' | 'info' | 'warning' | 'danger' | 'secondary' | 'contrast' | undefined {
    switch (status) {
      case SprintStatus.Active:
        return 'success';
      case SprintStatus.Completed:
        return 'info';
      case SprintStatus.Planned:
        return 'warning';
      default:
        return 'secondary';
    }
  }

  formatDateRange(start: string, end: string): string {
    const startDate = new Date(start);
    const endDate = new Date(end);
    const options: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric' };
    return `${startDate.toLocaleDateString('en-US', options)} - ${endDate.toLocaleDateString('en-US', options)}`;
  }
}
