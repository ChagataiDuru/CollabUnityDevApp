import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { TaskService } from '../../../core/services/task.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { Task, TaskColumn } from '../../../core/models/task.model';
import { Subscription } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TaskDetailComponent } from '../task-detail/task-detail.component';

import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-kanban-board',
  standalone: true,
  imports: [CommonModule, DragDropModule, ButtonModule, DialogModule, InputTextModule, ReactiveFormsModule, TaskDetailComponent, SkeletonModule],
  template: `
    <div class="h-full flex flex-col bg-slate-900 text-slate-100 p-4 overflow-hidden">
      <div class="flex justify-between items-center mb-4">
        <h2 class="text-2xl font-bold">Board</h2>
        <div class="flex items-center gap-3">
            <button pButton label="Team" icon="pi pi-users" (click)="navigateToTeamSettings()" 
                class="p-button-outlined p-button-sm"></button>
            <button pButton label="Whiteboard" icon="pi pi-pencil" (click)="navigateToWhiteboard()" 
                class="p-button-outlined p-button-secondary p-button-sm"></button>
            <button pButton label="Integrations" icon="pi pi-github" (click)="navigateToIntegrations()" 
                class="p-button-outlined p-button-secondary p-button-sm"></button>
            <button pButton label="Sprints" icon="pi pi-calendar" (click)="navigateToSprints()" 
                class="p-button-outlined p-button-secondary p-button-sm"></button>
            <button pButton label="Wiki" icon="pi pi-book" (click)="navigateToWiki()"
                class="p-button-outlined p-button-secondary p-button-sm"></button>
            <button pButton label="Dashboard" icon="pi pi-chart-bar" (click)="navigateToAnalytics()"
                class="p-button-outlined p-button-secondary p-button-sm"></button>
            <p-button label="Add Column" icon="pi pi-plus" (onClick)="showAddColumnDialog()" styleClass="p-button-sm"></p-button>
        </div>
      </div>

      <!-- Skeletons -->
      <div *ngIf="isLoadingBoard" class="flex-1 flex gap-4 overflow-x-auto pb-4">
        <div *ngFor="let i of [1,2,3]" class="w-80 flex-shrink-0 flex flex-col bg-slate-800 rounded-xl border border-slate-700 h-full p-3">
            <div class="flex justify-between items-center mb-4">
                <p-skeleton width="60%" height="1.5rem"></p-skeleton>
                <p-skeleton width="2rem" height="1rem" borderRadius="16px"></p-skeleton>
            </div>
            <div class="flex flex-col gap-3">
                <p-skeleton width="100%" height="8rem" borderRadius="8px"></p-skeleton>
                <p-skeleton width="100%" height="6rem" borderRadius="8px"></p-skeleton>
                <p-skeleton width="100%" height="8rem" borderRadius="8px"></p-skeleton>
            </div>
        </div>
      </div>

      <div *ngIf="!isLoadingBoard" class="flex-1 flex gap-4 overflow-x-auto pb-4" cdkDropListGroup>
        @for (column of columns; track column.id) {
          <div class="w-80 flex-shrink-0 flex flex-col bg-slate-800 rounded-xl border border-slate-700 max-h-full">
            <div class="p-3 flex justify-between items-center border-b border-slate-700" [style.borderTop]="'4px solid ' + column.color">
              <h3 class="font-semibold">{{ column.name }}</h3>
              <span class="text-xs text-slate-400 bg-slate-700 px-2 py-0.5 rounded-full">{{ getTasksByColumn(column.id).length }}</span>
            </div>

            <div class="flex-1 overflow-y-auto p-2 min-h-0"
                 cdkDropList
                 [cdkDropListData]="getTasksByColumn(column.id)"
                 (cdkDropListDropped)="drop($event, column.id)">
              
              @for (task of getTasksByColumn(column.id); track task.id) {
                <div class="bg-slate-700 p-3 rounded-lg mb-2 shadow-sm cursor-pointer hover:bg-slate-600 transition-colors border border-slate-600" cdkDrag (click)="openTaskDetail(task)">
                  <div class="flex justify-between items-start mb-2">
                    <span class="text-sm font-medium">{{ task.title }}</span>
                    <div class="w-2 h-2 rounded-full" [class.bg-red-500]="task.priority === 2" [class.bg-yellow-500]="task.priority === 1" [class.bg-green-500]="task.priority === 0"></div>
                  </div>
                  
                  <div class="flex justify-between items-center mt-3">
                    <div class="flex gap-2 text-xs text-slate-400">
                      <span *ngIf="task.commentsCount > 0"><i class="pi pi-comment mr-1"></i>{{ task.commentsCount }}</span>
                      <span *ngIf="task.attachmentsCount > 0"><i class="pi pi-paperclip mr-1"></i>{{ task.attachmentsCount }}</span>
                      <span *ngIf="task.checklistTotal > 0"><i class="pi pi-check-square mr-1"></i>{{ task.checklistCompleted }}/{{ task.checklistTotal }}</span>
                    </div>
                    
                    <div class="flex -space-x-2">
                        <div *ngFor="let tag of task.tags" class="w-2 h-2 rounded-full" [style.background-color]="tag.color" [title]="tag.name"></div>
                    </div>

                    <div *ngIf="task.assignedToName" class="w-6 h-6 rounded-full bg-indigo-500 flex items-center justify-center text-xs text-white" [title]="task.assignedToName">
                      {{ task.assignedToName.charAt(0) }}
                    </div>
                  </div>
                </div>
              }
            </div>
            
            <div class="p-2 border-t border-slate-700">
              <button class="w-full py-2 text-slate-400 hover:text-slate-200 hover:bg-slate-700 rounded-lg text-sm transition-colors flex items-center justify-center gap-2" (click)="showAddTaskDialog(column.id)">
                <i class="pi pi-plus"></i> Add Task
              </button>
            </div>
          </div>
        }
      </div>

      <p-dialog header="New Column" [(visible)]="displayAddColumn" [modal]="true" [style]="{width: '300px'}">
        <form [formGroup]="columnForm" (ngSubmit)="createColumn()">
          <div class="field mb-4">
            <label for="colName" class="block text-sm font-medium text-slate-300 mb-1">Name</label>
            <input pInputText id="colName" formControlName="name" class="w-full" />
          </div>
          <p-button label="Create" type="submit" [loading]="loading" styleClass="w-full"></p-button>
        </form>
      </p-dialog>

      <p-dialog header="New Task" [(visible)]="displayAddTask" [modal]="true" [style]="{width: '400px'}">
        <form [formGroup]="taskForm" (ngSubmit)="createTask()">
          <div class="field mb-4">
            <label for="title" class="block text-sm font-medium text-slate-300 mb-1">Title</label>
            <input pInputText id="title" formControlName="title" class="w-full" />
          </div>
          <div class="field mb-4">
            <label for="content" class="block text-sm font-medium text-slate-300 mb-1">Description</label>
            <textarea pInputText id="content" formControlName="content" class="w-full" rows="3"></textarea>
          </div>
           <div class="field mb-4">
            <label for="priority" class="block text-sm font-medium text-slate-300 mb-1">Priority</label>
            <select formControlName="priority" class="w-full p-2 bg-slate-700 border border-slate-600 rounded text-slate-200">
                <option [value]="0">Low</option>
                <option [value]="1">Medium</option>
                <option [value]="2">High</option>
            </select>
          </div>
          <p-button label="Create Task" type="submit" [loading]="loading" styleClass="w-full"></p-button>
        </form>
      </p-dialog>

      <app-task-detail
        [(visible)]="displayTaskDetail"
        [task]="selectedTask"
        [columns]="columns"
        (taskUpdated)="onTaskUpdated($event)"
        (taskDeleted)="onTaskDeleted($event)">
      </app-task-detail>
    </div>
  `,
  styles: [`
    .cdk-drag-preview {
      box-shadow: 0 10px 20px rgba(0,0,0,0.19), 0 6px 6px rgba(0,0,0,0.23);
      transform: rotate(2deg);
      background-color: #334155;
      border-radius: 0.5rem;
      opacity: 0.95;
    }
    .cdk-drag-placeholder {
      opacity: 0.3;
      border: 2px dashed #94a3b8;
      background: transparent;
    }
    .cdk-drag-animating {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }
    .cdk-drop-list-dragging .cdk-drag {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }
  `]
})
/**
 * Component for the Kanban board, managing columns and tasks.
 */
export class KanbanBoardComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private taskService = inject(TaskService);
  private signalRService = inject(SignalRService);
  private fb = inject(FormBuilder);

  projectId: string = '';
  columns: TaskColumn[] = [];
  tasks: Task[] = [];

  displayAddTask = false;
  displayAddColumn = false;
  displayTaskDetail = false;
  loading = false;
  isLoadingBoard = true;
  selectedColumnId = '';
  selectedTask: Task | null = null;

  columnForm = this.fb.group({
    name: ['', Validators.required],
    color: ['#6366f1']
  });

  taskForm = this.fb.group({
    title: ['', Validators.required],
    content: [''],
    priority: [0]
  });

  private signalRSub: Subscription | null = null;

  /**
   * Initializes the component, loads the board data, and starts SignalR connection.
   */
  ngOnInit() {
    this.projectId = this.route.snapshot.paramMap.get('id')!;
    this.loadBoard();
    this.signalRService.startConnection(this.projectId);

    this.signalRSub = this.signalRService.projectEvents$.subscribe(event => {
      if (event) this.handleRealTimeEvent(event);
    });
  }

  /**
   * Cleans up resources, including the SignalR connection.
   */
  ngOnDestroy() {
    this.signalRService.stopConnection();
    this.signalRSub?.unsubscribe();
  }

  /**
   * Navigates to the whiteboard view.
   */
  navigateToWhiteboard() {
    this.router.navigate(['projects', this.projectId, 'whiteboard']);
  }

  /**
   * Navigates to the team settings view.
   */
  navigateToTeamSettings() {
    this.router.navigate(['projects', this.projectId, 'settings']);
  }

  /**
   * Navigates to the integrations view.
   */
  navigateToIntegrations() {
    this.router.navigate(['projects', this.projectId, 'integrations']);
  }

  /**
   * Navigates to the sprints view.
   */
  navigateToSprints() {
    this.router.navigate(['projects', this.projectId, 'sprints']);
  }

  /**
   * Navigates to the wiki view.
   */
  navigateToWiki() {
    this.router.navigate(['projects', this.projectId, 'wiki']);
  }

  /**
   * Navigates to the analytics view.
   */
  navigateToAnalytics() {
    this.router.navigate(['projects', this.projectId, 'analytics']);
  }

  /**
   * Loads columns and tasks from the backend.
   */
  loadBoard() {
    this.isLoadingBoard = true;
    this.taskService.getColumns(this.projectId).subscribe(cols => {
      this.columns = cols;
      this.taskService.getTasks(this.projectId).subscribe(tasks => {
        this.tasks = tasks;
        this.isLoadingBoard = false;
      });
    });
  }

  /**
   * Filters and sorts tasks for a specific column.
   * @param columnId The column ID.
   * @returns An array of tasks in the column.
   */
  getTasksByColumn(columnId: string) {
    return this.tasks.filter(t => t.columnId === columnId).sort((a, b) => a.position - b.position);
  }

  /**
   * Handles drag-and-drop events for tasks.
   * @param event The drag-drop event.
   * @param columnId The target column ID.
   */
  drop(event: CdkDragDrop<Task[]>, columnId: string) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      const task = event.container.data[event.currentIndex];
      this.taskService.moveTask(task.id, { newColumnId: columnId, newPosition: event.currentIndex }).subscribe();
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex,
      );
      const task = event.container.data[event.currentIndex];
      task.columnId = columnId;
      this.taskService.moveTask(task.id, { newColumnId: columnId, newPosition: event.currentIndex }).subscribe();
    }
  }

  /**
   * Opens the dialog for adding a new column.
   */
  showAddColumnDialog() {
    this.columnForm.reset({ name: '', color: '#6366f1' });
    this.displayAddColumn = true;
  }

  /**
   * Creates a new column.
   */
  createColumn() {
    if (this.columnForm.valid) {
      this.loading = true;
      this.taskService.createColumn(this.projectId, this.columnForm.value as any).subscribe({
        next: (col) => {
          if (!this.columns.find(c => c.id === col.id)) {
            this.columns.push(col);
          }
          this.displayAddColumn = false;
          this.loading = false;
        },
        error: () => this.loading = false
      });
    }
  }

  /**
   * Opens the dialog for adding a new task.
   * @param columnId The column ID where the task will be added.
   */
  showAddTaskDialog(columnId: string) {
    this.selectedColumnId = columnId;
    this.taskForm.reset({ title: '', content: '', priority: 0 });
    this.displayAddTask = true;
  }

  /**
   * Creates a new task.
   */
  createTask() {
    if (this.taskForm.valid) {
      this.loading = true;
      const formValue = this.taskForm.value;
      const taskData = {
        title: formValue.title,
        description: formValue.content,
        priority: Number(formValue.priority),
        columnId: this.selectedColumnId
      };

      this.taskService.createTask(this.projectId, taskData as any).subscribe({
        next: (task) => {
          if (!this.tasks.find(t => t.id === task.id)) {
            this.tasks.push(task);
          }
          this.displayAddTask = false;
          this.loading = false;
        },
        error: () => this.loading = false
      });
    }
  }

  /**
   * Opens the task detail view.
   * @param task The selected task.
   */
  openTaskDetail(task: Task) {
    this.selectedTask = task;
    this.displayTaskDetail = true;
  }

  /**
   * Updates a task in the local state.
   * @param task The updated task.
   */
  onTaskUpdated(task: Task) {
    const index = this.tasks.findIndex(t => t.id === task.id);
    if (index !== -1) {
      this.tasks[index] = task;
    }
  }

  /**
   * Removes a task from the local state.
   * @param taskId The ID of the deleted task.
   */
  onTaskDeleted(taskId: string) {
    this.tasks = this.tasks.filter(t => t.id !== taskId);
    this.displayTaskDetail = false;
    this.selectedTask = null;
  }

  /**
   * Handles real-time events from SignalR.
   * @param event The event data.
   */
  handleRealTimeEvent(event: any) {
    switch (event.type) {
      case 'TaskCreated':
        if (!this.tasks.find(t => t.id === event.data.id)) {
          this.tasks.push(event.data);
        }
        break;
      case 'TaskUpdated':
        const idx = this.tasks.findIndex(t => t.id === event.data.id);
        if (idx !== -1) {
          this.tasks[idx] = event.data;
          if (this.selectedTask && this.selectedTask.id === event.data.id) {
            this.selectedTask = event.data;
          }
        }
        break;
      case 'TaskMoved':
        const taskId = event.data;
        const newColId = event.args?.[0];

        if (taskId && newColId) {
          const task = this.tasks.find(t => t.id === taskId);
          if (task) task.columnId = newColId;
        }
        break;
      case 'ColumnCreated':
        if (!this.columns.find(c => c.id === event.data.id)) {
          this.columns.push(event.data);
        }
        break;
    }
  }
}
