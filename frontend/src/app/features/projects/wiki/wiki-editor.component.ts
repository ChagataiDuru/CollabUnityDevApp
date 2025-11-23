import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EditorModule } from 'primeng/editor';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-wiki-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, EditorModule, ButtonModule, InputTextModule],
  template: `
    <div class="flex flex-col h-full gap-4">
      <div class="flex flex-col gap-2">
        <label for="title" class="font-medium">Title</label>
        <input pInputText id="title" [(ngModel)]="title" class="w-full" placeholder="Page Title" />
      </div>

      <div class="flex-1 flex flex-col gap-2 min-h-0">
        <label for="content" class="font-medium">Content</label>
        <p-editor [(ngModel)]="content" [style]="{ height: '100%' }" class="flex-1"></p-editor>
      </div>

      <div class="flex justify-end gap-2 mt-4">
        <p-button label="Cancel" icon="pi pi-times" styleClass="p-button-text" (onClick)="onCancel.emit()"></p-button>
        <p-button label="Save" icon="pi pi-save" (onClick)="save()" [disabled]="!title"></p-button>
      </div>
    </div>
  `,
  styles: [`
    :host ::ng-deep .p-editor-container {
      display: flex;
      flex-direction: column;
      height: 100%;
    }
    :host ::ng-deep .p-editor-content {
      flex: 1;
      overflow: auto;
    }
  `]
})
export class WikiEditorComponent {
  @Input() title: string = '';
  @Input() content: string = '';

  @Output() onSave = new EventEmitter<{title: string, content: string}>();
  @Output() onCancel = new EventEmitter<void>();

  save() {
    this.onSave.emit({
      title: this.title,
      content: this.content
    });
  }
}
