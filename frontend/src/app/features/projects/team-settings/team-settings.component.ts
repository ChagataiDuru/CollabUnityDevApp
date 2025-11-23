import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ProjectMemberService } from '../../../core/services/project-member.service';
import { ProjectMember, ProjectRole, UserDto } from '../../../core/models/project-member.model';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';

@Component({
    selector: 'app-team-settings',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        ButtonModule,
        TableModule,
        DialogModule,
        InputTextModule,
        DropdownModule,
        ToastModule
    ],
    providers: [MessageService],
    templateUrl: './team-settings.component.html',
    styleUrls: ['./team-settings.component.css']
})
export class TeamSettingsComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private memberService = inject(ProjectMemberService);
    private messageService = inject(MessageService);

    projectId: string = '';
    members: ProjectMember[] = [];
    loading = false;

    // Add Member Dialog
    showAddMemberDialog = false;
    searchQuery = '';
    searchResults: UserDto[] = [];
    searching = false;
    selectedRole: ProjectRole = ProjectRole.Member;

    private searchSubject = new Subject<string>();

    roleOptions = [
        { label: 'Admin', value: ProjectRole.Admin },
        { label: 'Member', value: ProjectRole.Member },
        { label: 'Viewer', value: ProjectRole.Viewer }
    ];

    ProjectRole = ProjectRole;

    ngOnInit() {
        this.projectId = this.route.snapshot.paramMap.get('id') || '';
        this.loadMembers();

        // Setup search debounce
        this.searchSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            switchMap(query => {
                this.searching = true;
                if (!query || query.length < 3) {
                    this.searching = false;
                    return [];
                }
                return this.memberService.searchUsers(query);
            })
        ).subscribe({
            next: (users) => {
                this.searchResults = users;
                this.searching = false;
            },
            error: () => {
                this.searchResults = [];
                this.searching = false;
            }
        });
    }

    loadMembers() {
        this.loading = true;
        this.memberService.getProjectMembers(this.projectId).subscribe({
            next: (members) => {
                this.members = members;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to load team members'
                });
            }
        });
    }

    openAddMemberDialog() {
        this.searchQuery = '';
        this.searchResults = [];
        this.selectedRole = ProjectRole.Member;
        this.showAddMemberDialog = true;
    }

    onSearch(event: any) {
        this.searchSubject.next(event.target.value);
    }

    addMember(user: UserDto) {
        this.memberService.addMember(this.projectId, {
            userId: user.id,
            role: this.selectedRole
        }).subscribe({
            next: (member) => {
                this.members.push(member);
                this.messageService.add({
                    severity: 'success',
                    summary: 'Member Added',
                    detail: `${user.username} added to project`
                });
                this.showAddMemberDialog = false;
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: err.error?.message || 'Failed to add member'
                });
            }
        });
    }

    updateRole(member: ProjectMember, newRole: ProjectRole) {
        this.memberService.updateMemberRole(this.projectId, member.userId, newRole).subscribe({
            next: () => {
                member.role = newRole;
                this.messageService.add({
                    severity: 'success',
                    summary: 'Role Updated',
                    detail: `${member.username}'s role updated`
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to update role'
                });
            }
        });
    }

    removeMember(member: ProjectMember) {
        if (confirm(`Remove ${member.username} from the project?`)) {
            this.memberService.removeMember(this.projectId, member.userId).subscribe({
                next: () => {
                    this.members = this.members.filter(m => m.id !== member.id);
                    this.messageService.add({
                        severity: 'success',
                        summary: 'Member Removed',
                        detail: `${member.username} removed from project`
                    });
                },
                error: () => {
                    this.messageService.add({
                        severity: 'error',
                        summary: 'Error',
                        detail: 'Failed to remove member'
                    });
                }
            });
        }
    }

    getRoleName(role: ProjectRole): string {
        return ProjectRole[role];
    }

    canModifyRole(member: ProjectMember): boolean {
        return member.role !== ProjectRole.Owner;
    }

    goToBoard() {
        this.router.navigate(['/projects', this.projectId]);
    }
}
