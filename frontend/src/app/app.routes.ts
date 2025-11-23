import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    {
        path: '',
        loadComponent: () => import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
        canActivate: [authGuard],
        children: [
            {
                path: 'projects/:id/settings',
                loadComponent: () => import('./features/projects/team-settings/team-settings.component').then(m => m.TeamSettingsComponent)
            },
            {
                path: 'projects/:id/integrations',
                loadComponent: () => import('./features/projects/integrations/integrations.component').then(m => m.IntegrationsComponent)
            },
            {
                path: 'projects/:projectId/sprints',
                loadComponent: () => import('./features/sprints/sprint-list/sprint-list.component').then(m => m.SprintListComponent)
            },
            {
                path: 'projects/:id/whiteboard',
                loadComponent: () => import('./features/whiteboard/whiteboard.component').then(m => m.WhiteboardComponent)
            },
            {
                path: 'projects/:id/wiki',
                loadComponent: () => import('./features/projects/wiki/wiki-layout.component').then(m => m.WikiLayoutComponent)
            },
            {
                path: 'projects/:id/analytics',
                loadComponent: () => import('./features/projects/analytics/project-analytics.component').then(m => m.ProjectAnalyticsComponent)
            },
            {
                path: 'projects',
                loadComponent: () => import('./features/projects/project-list/project-list.component').then(m => m.ProjectListComponent)
            },
            {
                path: 'projects/:id',
                loadComponent: () => import('./features/tasks/kanban-board/kanban-board.component').then(m => m.KanbanBoardComponent)
            },
            { path: '', redirectTo: 'projects', pathMatch: 'full' }
        ]
    },
    { path: '**', redirectTo: 'projects' }
];
