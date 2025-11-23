import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { WikiPage } from './wiki.service';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-wiki-page',
  standalone: true,
  imports: [CommonModule, ButtonModule, CardModule, DatePipe],
  template: `
    <div class="h-full flex flex-col">
      <div class="flex justify-between items-start mb-6 border-b pb-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-800 mb-2">{{ page.title }}</h1>
          <div class="text-sm text-gray-500 flex gap-4">
            <span>Last edited by {{ page.lastEditorName || 'Unknown' }}</span>
            <span>{{ page.updatedAt | date:'medium' }}</span>
          </div>
        </div>
        <div class="flex gap-2">
            <p-button label="Edit" icon="pi pi-pencil" (onClick)="onEdit.emit(page)"></p-button>
            <p-button label="Delete" icon="pi pi-trash" styleClass="p-button-danger p-button-outlined" (onClick)="onDelete.emit(page)"></p-button>
        </div>
      </div>

      <div class="prose max-w-none flex-1 overflow-y-auto" [innerHTML]="page.content">
      </div>
    </div>
  `,
  styles: []
})
export class WikiPageComponent {
  @Input() page!: WikiPage;
  @Output() onEdit = new EventEmitter<WikiPage>();
  @Output() onDelete = new EventEmitter<WikiPage>();
}
