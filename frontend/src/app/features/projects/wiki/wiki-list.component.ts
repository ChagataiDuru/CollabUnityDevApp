import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TreeModule } from 'primeng/tree';
import { TreeNode } from 'primeng/api';
import { WikiPage } from './wiki.service';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-wiki-list',
  standalone: true,
  imports: [CommonModule, TreeModule, ButtonModule, TooltipModule],
  template: `
    <div class="h-full flex flex-col">
      <div class="flex justify-between items-center mb-4 px-2">
        <h3 class="font-semibold text-gray-700">Pages</h3>
        <p-button icon="pi pi-plus" size="small" [rounded]="true" [text]="true" pTooltip="New Page" (onClick)="onCreateRoot()"></p-button>
      </div>

      <p-tree
        [value]="files"
        selectionMode="single"
        [(selection)]="selectedNode"
        (onNodeSelect)="nodeSelect($event)"
        class="w-full border-none p-0"
        [style]="{'border': 'none', 'padding': '0'}">
        <ng-template let-node pTemplate="default">
          <div class="flex items-center justify-between w-full group">
            <span class="truncate pr-2">{{node.label}}</span>
            <div class="hidden group-hover:flex">
                <button
                    pButton
                    icon="pi pi-plus"
                    class="p-button-text p-button-rounded p-button-sm w-6 h-6"
                    (click)="onCreateChild($event, node)"
                    pTooltip="Add Sub-page">
                </button>
            </div>
          </div>
        </ng-template>
      </p-tree>
    </div>
  `,
  styles: [`
    :host ::ng-deep .p-tree {
      border: none;
      padding: 0;
    }
    :host ::ng-deep .p-treenode-content {
      padding: 0.25rem 0;
    }
  `]
})
export class WikiListComponent {
  @Input() set pages(value: WikiPage[]) {
    this.nodes = this.transformToTreeNodes(value);
  }
  @Output() onSelect = new EventEmitter<WikiPage>();
  @Output() onCreate = new EventEmitter<WikiPage | null>(); // null for root

  nodes: TreeNode[] = [];
  selectedNode: TreeNode | null = null;

  transformToTreeNodes(pages: WikiPage[]): TreeNode[] {
    return pages.map(page => ({
      key: page.id,
      label: page.title,
      data: page,
      expandedIcon: 'pi pi-folder-open',
      collapsedIcon: 'pi pi-folder',
      children: page.children ? this.transformToTreeNodes(page.children) : [],
      expanded: true // Expand by default for now
    }));
  }

  nodeSelect(event: any) {
    this.onSelect.emit(event.node.data);
  }

  onCreateRoot() {
    this.onCreate.emit(null);
  }

  onCreateChild(event: Event, node: TreeNode) {
    event.stopPropagation();
    this.onCreate.emit(node.data);
  }
}
