import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { WikiService, WikiPage } from './wiki.service';
import { WikiListComponent } from './wiki-list.component';
import { WikiPageComponent } from './wiki-page.component';
import { WikiEditorComponent } from './wiki-editor.component';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-wiki-layout',
  standalone: true,
  imports: [
    CommonModule,
    WikiListComponent,
    WikiPageComponent,
    WikiEditorComponent,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule
  ],
  providers: [ConfirmationService, MessageService],
  template: `
    <div class="flex h-full bg-white rounded-lg shadow-sm overflow-hidden">
      <!-- Sidebar -->
      <div class="w-80 border-r bg-gray-50 p-4 flex flex-col overflow-y-auto">
        <app-wiki-list
            [pages]="pages"
            (onSelect)="selectPage($event)"
            (onCreate)="startCreate($event)">
        </app-wiki-list>
      </div>

      <!-- Main Content -->
      <div class="flex-1 p-8 overflow-y-auto relative">
        <p-toast></p-toast>
        <p-confirmDialog></p-confirmDialog>

        <div *ngIf="loading" class="absolute inset-0 flex items-center justify-center bg-white bg-opacity-75 z-10">
            <p-progress-spinner></p-progress-spinner>
        </div>

        <ng-container *ngIf="!loading">
            <!-- View Mode -->
            <app-wiki-page
                *ngIf="selectedPage && !isEditing"
                [page]="selectedPage"
                (onEdit)="startEdit()"
                (onDelete)="confirmDelete($event)">
            </app-wiki-page>

            <!-- Edit/Create Mode -->
            <app-wiki-editor
                *ngIf="isEditing"
                [title]="editTitle"
                [content]="editContent"
                (onSave)="savePage($event)"
                (onCancel)="cancelEdit()">
            </app-wiki-editor>

            <!-- Empty State -->
            <div *ngIf="!selectedPage && !isEditing" class="flex flex-col items-center justify-center h-full text-gray-500">
                <i class="pi pi-book text-6xl mb-4 text-gray-300"></i>
                <p class="text-xl">Select a page or create a new one to get started.</p>
            </div>
        </ng-container>
      </div>
    </div>
  `
})
export class WikiLayoutComponent implements OnInit {
  projectId: string = '';
  pages: WikiPage[] = [];
  selectedPage: WikiPage | null = null;

  isEditing: boolean = false;
  isCreating: boolean = false;
  parentPageId: string | undefined = undefined; // For creating child pages

  editTitle: string = '';
  editContent: string = '';

  loading: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private wikiService: WikiService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.route.parent?.params.subscribe(params => {
        this.projectId = params['id'];
        if (this.projectId) {
            this.loadPages();
        }
    });
  }

  loadPages() {
    this.loading = true;
    this.wikiService.getProjectWiki(this.projectId).subscribe({
        next: (pages) => {
            this.pages = pages;
            this.loading = false;
            // If we have a selected page, try to refresh it from the list
            if (this.selectedPage) {
               // Finding deeply nested page is tricky with tree structure returned,
               // but usually we might just re-fetch the single page or traversing the tree.
               // For now, let's just keep the object or reload it if needed.
               // Actually, `pages` is the root list.
            }
        },
        error: (err) => {
            this.messageService.add({severity:'error', summary:'Error', detail:'Failed to load wiki pages'});
            this.loading = false;
        }
    });
  }

  selectPage(page: WikiPage) {
    if (this.isEditing) {
        this.confirmationService.confirm({
            message: 'You have unsaved changes. Are you sure you want to switch pages?',
            accept: () => {
                this.isEditing = false;
                this.selectedPage = page;
            }
        });
    } else {
        this.selectedPage = page;
    }
  }

  startCreate(parent: WikiPage | null) {
    this.isCreating = true;
    this.isEditing = true;
    this.parentPageId = parent ? parent.id : undefined;
    this.editTitle = '';
    this.editContent = '';
    this.selectedPage = null; // Clear selection to show editor
  }

  startEdit() {
    if (!this.selectedPage) return;
    this.isCreating = false;
    this.isEditing = true;
    this.editTitle = this.selectedPage.title;
    this.editContent = this.selectedPage.content;
  }

  cancelEdit() {
    this.isEditing = false;
    this.isCreating = false;
    // If we were creating, go back to nothing or previous selection?
    // If editing, go back to view.
  }

  savePage(data: {title: string, content: string}) {
    if (!this.projectId) return;

    this.loading = true;

    if (this.isCreating) {
        this.wikiService.createPage(this.projectId, {
            title: data.title,
            content: data.content,
            parentId: this.parentPageId
        }).subscribe({
            next: (newPage) => {
                this.messageService.add({severity:'success', summary:'Success', detail:'Page created'});
                this.isEditing = false;
                this.isCreating = false;
                this.selectedPage = newPage;
                this.loadPages(); // Refresh tree
            },
            error: (err) => {
                this.messageService.add({severity:'error', summary:'Error', detail:'Failed to create page'});
                this.loading = false;
            }
        });
    } else if (this.selectedPage) {
        this.wikiService.updatePage(this.selectedPage.id, {
            title: data.title,
            content: data.content,
            parentId: this.selectedPage.parentId // Keep same parent for now
        }).subscribe({
            next: (updatedPage) => {
                this.messageService.add({severity:'success', summary:'Success', detail:'Page updated'});
                this.isEditing = false;
                this.selectedPage = updatedPage;
                this.loadPages(); // Refresh tree
            },
            error: (err) => {
                this.messageService.add({severity:'error', summary:'Error', detail:'Failed to update page'});
                this.loading = false;
            }
        });
    }
  }

  confirmDelete(page: WikiPage) {
    this.confirmationService.confirm({
        message: `Are you sure you want to delete "${page.title}"?`,
        header: 'Delete Confirmation',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
            this.deletePage(page);
        }
    });
  }

  deletePage(page: WikiPage) {
    this.loading = true;
    this.wikiService.deletePage(page.id).subscribe({
        next: () => {
            this.messageService.add({severity:'success', summary:'Success', detail:'Page deleted'});
            this.selectedPage = null;
            this.loadPages();
        },
        error: (err) => {
            this.messageService.add({severity:'error', summary:'Error', detail:'Failed to delete page'});
            this.loading = false;
        }
    });
  }
}
