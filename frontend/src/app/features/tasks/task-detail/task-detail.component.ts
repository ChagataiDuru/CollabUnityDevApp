import { Component, EventEmitter, Input, Output, inject, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Task, TaskColumn, ChecklistItem, TaskComment, TaskAttachment, TaskTag } from '../../../core/models/task.model';
import { User } from '../../../core/models/user.model';
import { TaskService } from '../../../core/services/task.service';
import { AuthService } from '../../../core/services/auth.service';
import { TimeLogService } from '../../../core/services/timelog.service';
import { TimeLog } from '../../../core/models/timelog.model';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { EditorModule } from 'primeng/editor';
import { CheckboxModule } from 'primeng/checkbox';
import { ProgressBarModule } from 'primeng/progressbar';
import { FileUploadModule } from 'primeng/fileupload';
import { ChipModule } from 'primeng/chip';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

@Component({
    selector: 'app-task-detail',
    standalone: true,
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        ButtonModule, InputTextModule, EditorModule, CheckboxModule,
        ProgressBarModule, FileUploadModule, ChipModule, AvatarModule,
        BadgeModule, TooltipModule, CalendarModule, DropdownModule,
        DialogModule, ConfirmDialogModule
    ],
    providers: [ConfirmationService],
    templateUrl: './task-detail.component.html',
    styleUrls: ['./task-detail.component.css']
})
export class TaskDetailComponent implements OnInit, OnChanges {
    @Input() task: Task | null = null;
    @Input() visible = false;
    @Input() columns: TaskColumn[] = [];
    @Output() visibleChange = new EventEmitter<boolean>();
    @Output() taskUpdated = new EventEmitter<Task>();
    @Output() taskDeleted = new EventEmitter<string>();

    private taskService = inject(TaskService);
    private authService = inject(AuthService);
    private fb = inject(FormBuilder);
    private confirmationService = inject(ConfirmationService);
    private timeLogService = inject(TimeLogService);

    currentUserId: string | null = null;
    currentUser: User | null = null;

    // Forms
    detailsForm = this.fb.group({
        title: ['', Validators.required],
        description: [''],
        priority: [0],
        dueDate: [null as Date | null],
        columnId: ['']
    });

    checklistForm = this.fb.group({
        title: ['', Validators.required]
    });

    commentForm = this.fb.group({
        content: ['', Validators.required]
    });

    tagForm = this.fb.group({
        name: ['', Validators.required]
    });

    manualTimeForm = this.fb.group({
        startTime: [null as Date | null, Validators.required],
        endTime: [null as Date | null, Validators.required],
        description: ['']
    });

    // State
    editMode = {
        title: false,
        description: false
    };

    priorities: any[] = [
        { label: 'Low', value: 0, color: 'success' },
        { label: 'Medium', value: 1, color: 'warning' },
        { label: 'High', value: 2, color: 'danger' }
    ];

    // Time Tracking State
    timeLogs: TimeLog[] = [];
    activeTimer: TimeLog | null = null;
    timerElapsed: string = '00:00:00';
    timerInterval: any = null;
    showManualTimeDialog = false;

    constructor() { }

    ngOnInit() {
        this.authService.currentUser$.subscribe(user => {
            this.currentUserId = user?.id || null;
            this.currentUser = user;
        });
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['task'] && this.task) {
            this.initForm();
            this.loadTimeLogs();
        }
    }

    ngOnDestroy() {
        this.stopTimerInterval();
    }

    initForm() {
        if (!this.task) return;

        this.detailsForm.patchValue({
            title: this.task.title,
            description: this.task.description,
            priority: this.task.priority,
            dueDate: this.task.dueDate ? new Date(this.task.dueDate) : null,
            columnId: this.task.columnId
        });
    }

    close() {
        this.visible = false;
        this.visibleChange.emit(false);
    }

    // Task Updates
    updateTaskField(field: string) {
        if (!this.task) return;

        const value = this.detailsForm.get(field)?.value;
        const updateDto: any = {};
        updateDto[field] = value;

        // Optimistic update
        (this.task as any)[field] = value;

        this.taskService.updateTask(this.task.id, { ...this.task, ...updateDto } as any).subscribe(updated => {
            this.task = updated;
            this.taskUpdated.emit(updated);
        });

        if (field === 'title') this.editMode.title = false;
        if (field === 'description') this.editMode.description = false;
    }

    deleteTask() {
        if (!this.task) return;

        this.confirmationService.confirm({
            message: 'Are you sure you want to delete this task?',
            accept: () => {
                this.taskService.deleteTask(this.task!.id).subscribe(() => {
                    this.taskDeleted.emit(this.task!.id);
                    this.close();
                });
            }
        });
    }

    // Checklist
    addChecklistItem() {
        if (!this.task || this.checklistForm.invalid) return;

        const title = this.checklistForm.get('title')?.value!;
        this.taskService.addChecklistItem(this.task.id, { title }).subscribe(item => {
            if (!this.task!.checklistItems) this.task!.checklistItems = [];
            this.task!.checklistItems.push(item);
            this.updateChecklistProgress();
            this.checklistForm.reset();
        });
    }

    toggleChecklistItem(item: ChecklistItem) {
        if (!this.task) return;

        item.isCompleted = !item.isCompleted;
        this.taskService.updateChecklistItem(this.task.id, item.id, { title: item.title, isCompleted: item.isCompleted }).subscribe(() => {
            this.updateChecklistProgress();
        });
    }

    deleteChecklistItem(itemId: string) {
        if (!this.task) return;

        this.taskService.deleteChecklistItem(this.task.id, itemId).subscribe(() => {
            this.task!.checklistItems = this.task!.checklistItems?.filter(i => i.id !== itemId);
            this.updateChecklistProgress();
        });
    }

    updateChecklistProgress() {
        if (!this.task || !this.task.checklistItems) return;
        const total = this.task.checklistItems.length;
        const completed = this.task.checklistItems.filter(i => i.isCompleted).length;
        this.task.checklistTotal = total;
        this.task.checklistCompleted = completed;
    }

    get checklistProgress(): number {
        if (!this.task || !this.task.checklistTotal) return 0;
        return Math.round((this.task.checklistCompleted / this.task.checklistTotal) * 100);
    }

    // Comments
    addComment() {
        if (!this.task || this.commentForm.invalid) return;

        const content = this.commentForm.get('content')?.value!;
        this.taskService.addComment(this.task.id, { content }).subscribe(comment => {
            if (!this.task!.comments) this.task!.comments = [];
            this.task!.comments.unshift(comment);
            this.commentForm.reset();
        });
    }

    deleteComment(commentId: string) {
        if (!this.task) return;

        this.taskService.deleteComment(this.task.id, commentId).subscribe(() => {
            this.task!.comments = this.task!.comments?.filter(c => c.id !== commentId);
        });
    }

    canDeleteComment(comment: TaskComment): boolean {
        return comment.userId === this.currentUserId;
    }

    // Tags
    addTag() {
        if (!this.task || this.tagForm.invalid) return;

        const name = this.tagForm.get('name')?.value!;
        // Generate random color or pick from palette
        const colors = ['#ef4444', '#f97316', '#eab308', '#22c55e', '#3b82f6', '#6366f1', '#a855f7', '#ec4899'];
        const color = colors[Math.floor(Math.random() * colors.length)];

        this.taskService.addTag(this.task.id, { name, color }).subscribe(tag => {
            if (!this.task!.tags) this.task!.tags = [];
            this.task!.tags.push(tag);
            this.tagForm.reset();
        });
    }

    deleteTag(tagId: string) {
        if (!this.task) return;

        this.taskService.deleteTag(this.task.id, tagId).subscribe(() => {
            this.task!.tags = this.task!.tags.filter(t => t.id !== tagId);
        });
    }

    // Attachments
    onFileUpload(event: any) {
        if (!this.task) return;

        // PrimeNG file upload handles the request, but we want to use our service
        // Or we can use the customUpload mode
        const file = event.files[0];
        this.taskService.uploadAttachment(this.task.id, file).subscribe(attachment => {
            if (!this.task!.attachments) this.task!.attachments = [];
            this.task!.attachments.unshift(attachment);
        });
    }

    deleteAttachment(attachmentId: string) {
        if (!this.task) return;

        this.taskService.deleteAttachment(this.task.id, attachmentId).subscribe(() => {
            this.task!.attachments = this.task!.attachments?.filter(a => a.id !== attachmentId);
        });
    }

    getPriorityColor(priority: number): any {
        switch (priority) {
            case 0: return 'success';
            case 1: return 'warning';
            case 2: return 'danger';
            default: return 'info';
        }
    }

    getPriorityLabel(priority: number): string {
        switch (priority) {
            case 0: return 'Low';
            case 1: return 'Medium';
            case 2: return 'High';
            default: return 'Unknown';
        }
    }

    getColumnName(columnId: string | undefined): string {
        if (!columnId) return '';
        return this.columns.find(c => c.id === columnId)?.name || '';
    }

    // Time Tracking
    loadTimeLogs() {
        if (!this.task) return;

        this.timeLogService.getTimeLogs(this.task.id).subscribe((logs: TimeLog[]) => {
            this.timeLogs = logs;
            // Check if there's an active timer
            this.activeTimer = logs.find(log => !log.endTime) || null;
            if (this.activeTimer) {
                this.startTimerInterval();
            }
        });
    }

    startTimer() {
        if (!this.task || this.activeTimer) return;

        this.timeLogService.startTimer(this.task.id, {}).subscribe((timeLog: TimeLog) => {
            this.activeTimer = timeLog;
            this.timeLogs.unshift(timeLog);
            this.startTimerInterval();
        });
    }

    stopTimer() {
        if (!this.activeTimer) return;

        this.timeLogService.stopTimer(this.activeTimer.id).subscribe((timeLog: TimeLog) => {
            // Update the time log in the list
            const index = this.timeLogs.findIndex(tl => tl.id === timeLog.id);
            if (index !== -1) {
                this.timeLogs[index] = timeLog;
            }
            this.activeTimer = null;
            this.stopTimerInterval();
        });
    }

    startTimerInterval() {
        if (!this.activeTimer?.startTime) return;

        this.timerInterval = setInterval(() => {
            const start = new Date(this.activeTimer!.startTime!);
            const now = new Date();
            const diff = now.getTime() - start.getTime();

            const hours = Math.floor(diff / 3600000);
            const minutes = Math.floor((diff % 3600000) / 60000);
            const seconds = Math.floor((diff % 60000) / 1000);

            this.timerElapsed = `${this.pad(hours)}:${this.pad(minutes)}:${this.pad(seconds)}`;
        }, 1000);
    }

    stopTimerInterval() {
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
            this.timerElapsed = '00:00:00';
        }
    }

    pad(num: number): string {
        return num.toString().padStart(2, '0');
    }

    showAddManualTime() {
        this.manualTimeForm.reset();
        this.showManualTimeDialog = true;
    }

    addManualTime() {
        if (!this.task || this.manualTimeForm.invalid) return;

        const startTime = this.manualTimeForm.get('startTime')?.value;
        const endTime = this.manualTimeForm.get('endTime')?.value;
        const description = this.manualTimeForm.get('description')?.value;

        this.timeLogService.createManualTimeLog(this.task.id, {
            startTime: startTime!.toISOString(),
            endTime: endTime!.toISOString(),
            description: description || undefined
        }).subscribe((timeLog: TimeLog) => {
            this.timeLogs.unshift(timeLog);
            this.showManualTimeDialog = false;
            this.manualTimeForm.reset();
        });
    }

    deleteTimeLog(timeLogId: string) {
        this.timeLogService.deleteTimeLog(timeLogId).subscribe(() => {
            this.timeLogs = this.timeLogs.filter(tl => tl.id !== timeLogId);
        });
    }

    formatDuration(minutes: number): string {
        const hours = Math.floor(minutes / 60);
        const mins = minutes % 60;
        if (hours > 0) {
            return `${hours}h ${mins}m`;
        }
        return `${mins}m`;
    }

    getTotalTimeLogged(): string {
        const totalMinutes = this.timeLogs.reduce((sum, log) => sum + log.durationMinutes, 0);
        return this.formatDuration(totalMinutes);
    }
}
