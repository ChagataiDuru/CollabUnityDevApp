import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { VcsService, RepositoryDto, RepositoryType } from '../../../core/services/vcs.service';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-integrations',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        TableModule,
        DialogModule,
        InputTextModule,
        DropdownModule,
        ToastModule
    ],
    providers: [MessageService],
    templateUrl: './integrations.component.html'
})
export class IntegrationsComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private vcsService = inject(VcsService);
    private messageService = inject(MessageService);
    private fb = inject(FormBuilder);

    projectId: string = '';
    repositories: RepositoryDto[] = [];
    loading = false;
    showAddDialog = false;
    addForm: FormGroup;

    repositoryTypes = [
        { label: 'GitHub', value: RepositoryType.GitHub },
        { label: 'GitLab', value: RepositoryType.GitLab }
    ];

    RepositoryType = RepositoryType;

    constructor() {
        this.addForm = this.fb.group({
            type: [RepositoryType.GitHub, Validators.required],
            url: ['', [Validators.required, Validators.pattern('https?://.+')]],
            webhookSecret: ['']
        });
    }

    ngOnInit() {
        // Get projectId from parent route or current route
        this.route.parent?.paramMap.subscribe(params => {
            const id = params.get('id');
            if (id) {
                this.projectId = id;
                this.loadRepositories();
            }
        });

        // Fallback if not child route
        if (!this.projectId) {
            this.projectId = this.route.snapshot.paramMap.get('id') || '';
            if (this.projectId) this.loadRepositories();
        }
    }

    loadRepositories() {
        this.loading = true;
        this.vcsService.getRepositories(this.projectId).subscribe({
            next: (repos) => {
                this.repositories = repos;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load repositories' });
            }
        });
    }

    openAddDialog() {
        this.addForm.reset({ type: RepositoryType.GitHub });
        this.showAddDialog = true;
    }

    onSubmit() {
        if (this.addForm.invalid) return;

        this.vcsService.addRepository(this.projectId, this.addForm.value).subscribe({
            next: (repo) => {
                this.repositories.push(repo);
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Repository added' });
                this.showAddDialog = false;
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to add repository' });
            }
        });
    }

    getWebhookUrl(repo: RepositoryDto): string {
        // Assuming the backend is reachable at the same host/api
        // In production, this should be the public URL of the API
        return `${window.location.origin}/api/webhooks/github`;
    }
}
