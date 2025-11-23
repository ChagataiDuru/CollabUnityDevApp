import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProjectService } from '../../../core/services/project.service';
import { Project } from '../../../core/models/project.model';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ColorPickerModule } from 'primeng/colorpicker';
import { CardModule } from 'primeng/card';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    InputTextareaModule,
    ColorPickerModule,
    CardModule,
    ToastModule
  ],
  providers: [MessageService],
  template: `
    <div class="min-h-screen bg-slate-900 p-6">
      <p-toast></p-toast>
      <div class="max-w-7xl mx-auto">
        <div class="flex justify-between items-center mb-8">
          <div>
            <h1 class="text-3xl font-bold text-slate-100">Projects</h1>
            <p class="text-slate-400 mt-1">Manage your game development projects</p>
          </div>
          <p-button label="New Project" icon="pi pi-plus" (onClick)="showCreateDialog()"></p-button>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          @for (project of projects; track project.id) {
            <div class="bg-slate-800 rounded-xl p-6 hover:bg-slate-750 transition-colors border border-slate-700 hover:border-indigo-500/50 group cursor-pointer relative" [routerLink]="['/projects', project.id]">
              <div class="absolute top-0 left-0 w-full h-1 rounded-t-xl" [style.backgroundColor]="project.colorTheme"></div>
              
              <div class="flex justify-between items-start mb-4">
                <h3 class="text-xl font-semibold text-slate-100">{{ project.name }}</h3>
                <button class="text-slate-400 hover:text-red-400 opacity-0 group-hover:opacity-100 transition-opacity" (click)="deleteProject($event, project.id)">
                  <i class="pi pi-trash"></i>
                </button>
              </div>
              
              <p class="text-slate-400 text-sm mb-6 line-clamp-2">{{ project.description || 'No description' }}</p>
              
              <div class="flex justify-between items-center text-xs text-slate-500">
                <span>Updated {{ project.updatedAt | date:'MMM d, y' }}</span>
                <span class="px-2 py-1 bg-slate-700 rounded-full text-slate-300">Owner</span>
              </div>
            </div>
          } @empty {
            <div class="col-span-full text-center py-12 bg-slate-800/50 rounded-xl border border-dashed border-slate-700">
              <i class="pi pi-folder-open text-4xl text-slate-600 mb-4"></i>
              <p class="text-slate-400">No projects found. Create your first one!</p>
            </div>
          }
        </div>
      </div>

      <p-dialog header="Create New Project" [(visible)]="displayCreateDialog" [modal]="true" [style]="{width: '450px'}" styleClass="p-fluid">
        <form [formGroup]="createForm" (ngSubmit)="createProject()">
          <div class="field mb-4">
            <label for="name" class="block text-sm font-medium text-slate-300 mb-1">Project Name</label>
            <input pInputText id="name" formControlName="name" class="w-full" />
          </div>
          
          <div class="field mb-4">
            <label for="description" class="block text-sm font-medium text-slate-300 mb-1">Description</label>
            <textarea pInputTextarea id="description" formControlName="description" rows="3" class="w-full"></textarea>
          </div>

          <div class="field mb-6">
            <label for="color" class="block text-sm font-medium text-slate-300 mb-1">Theme Color</label>
            <p-colorPicker formControlName="colorTheme" [inline]="false"></p-colorPicker>
          </div>

          <div class="flex justify-end gap-2">
            <p-button label="Cancel" styleClass="p-button-text" (onClick)="displayCreateDialog = false"></p-button>
            <p-button label="Create" type="submit" [loading]="loading"></p-button>
          </div>
        </form>
      </p-dialog>
    </div>
  `
})
/**
 * Component for displaying the list of projects and managing project creation/deletion.
 */
export class ProjectListComponent implements OnInit {
  private projectService = inject(ProjectService);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);

  projects: Project[] = [];
  displayCreateDialog = false;
  loading = false;

  createForm = this.fb.group({
    name: ['', [Validators.required]],
    description: [''],
    colorTheme: ['#6366f1']
  });

  /**
   * Initializes the component and loads projects.
   */
  ngOnInit() {
    this.loadProjects();
  }

  /**
   * Loads the list of projects from the service.
   */
  loadProjects() {
    this.projectService.getProjects().subscribe({
      next: (data) => this.projects = data,
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load projects' })
    });
  }

  /**
   * Opens the dialog for creating a new project.
   */
  showCreateDialog() {
    this.createForm.reset({ colorTheme: '#6366f1' });
    this.displayCreateDialog = true;
  }

  /**
   * Submits the project creation form.
   */
  createProject() {
    if (this.createForm.valid) {
      this.loading = true;
      this.projectService.createProject(this.createForm.value as any).subscribe({
        next: (project) => {
          this.projects.unshift(project);
          this.displayCreateDialog = false;
          this.loading = false;
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Project created' });
        },
        error: () => {
          this.loading = false;
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to create project' });
        }
      });
    }
  }

  /**
   * Deletes a project.
   * @param event The click event to stop propagation.
   * @param id The ID of the project to delete.
   */
  deleteProject(event: Event, id: string) {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this project?')) {
      this.projectService.deleteProject(id).subscribe({
        next: () => {
          this.projects = this.projects.filter(p => p.id !== id);
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Project deleted' });
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete project' })
      });
    }
  }
}
