import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentationService, SearchResult, PinnedDoc, SearchHistory } from '../../core/services/documentation.service';
import { ProjectService } from '../../core/services/project.service';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-documentation-panel',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        InputTextModule,
        TooltipModule,
        ProgressSpinnerModule,
        ToastModule
    ],
    templateUrl: './documentation-panel.component.html',
    styles: [`
    :host {
      display: block;
      position: fixed;
      top: 64px; /* Navbar height */
      right: 0;
      bottom: 0;
      width: 400px;
      background-color: #0f172a; /* Slate 900 */
      border-left: 1px solid #334155; /* Slate 700 */
      z-index: 40;
      transform: translateX(100%);
      transition: transform 0.3s ease-in-out;
      box-shadow: -4px 0 15px rgba(0, 0, 0, 0.3);
    }

    :host.visible {
      transform: translateX(0);
    }
  `]
})
export class DocumentationPanelComponent implements OnInit {
    docService = inject(DocumentationService);
    projectService = inject(ProjectService);
    messageService = inject(MessageService);

    searchQuery = '';
    searchResults: SearchResult[] = [];
    pinnedDocs: PinnedDoc[] = [];
    searchHistory: SearchHistory[] = [];

    loading = false;
    activeTab: 'search' | 'pinned' = 'search';

    ngOnInit() {
        this.loadHistory();
        // Load pinned docs if we have a project context
        if (this.projectService.currentProject()) {
            this.loadPinnedDocs();
        }
    }

    get isVisible() {
        return this.docService.panelVisible();
    }

    close() {
        this.docService.panelVisible.set(false);
    }

    search() {
        if (!this.searchQuery.trim()) return;

        this.loading = true;
        this.activeTab = 'search';

        this.docService.search(this.searchQuery).subscribe({
            next: (results) => {
                this.searchResults = results;
                this.loading = false;
                this.loadHistory(); // Refresh history
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Search failed' });
            }
        });
    }

    loadPinnedDocs() {
        const projectId = this.projectService.currentProject()?.id;
        if (!projectId) return;

        this.docService.getPinnedDocs(projectId).subscribe(docs => {
            this.pinnedDocs = docs;
        });
    }

    loadHistory() {
        this.docService.getHistory().subscribe(history => {
            this.searchHistory = history;
        });
    }

    pinResult(result: SearchResult) {
        const projectId = this.projectService.currentProject()?.id;
        if (!projectId) {
            this.messageService.add({ severity: 'warn', summary: 'No Project', detail: 'Open a project to pin documentation.' });
            return;
        }

        this.docService.pinDoc(projectId, {
            title: result.title,
            url: result.link,
            description: result.snippet
        }).subscribe({
            next: (doc) => {
                this.pinnedDocs.unshift(doc);
                this.messageService.add({ severity: 'success', summary: 'Pinned', detail: 'Documentation pinned to project.' });
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to pin documentation.' });
            }
        });
    }

    unpinDoc(id: string) {
        this.docService.unpinDoc(id).subscribe(() => {
            this.pinnedDocs = this.pinnedDocs.filter(d => d.id !== id);
            this.messageService.add({ severity: 'success', summary: 'Unpinned', detail: 'Documentation removed.' });
        });
    }

    useHistory(query: string) {
        this.searchQuery = query;
        this.search();
    }
}
