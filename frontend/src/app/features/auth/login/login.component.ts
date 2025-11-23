import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CardModule } from 'primeng/card';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ButtonModule, InputTextModule, PasswordModule, CardModule, ToastModule],
  providers: [MessageService],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-slate-900 p-4">
      <p-toast></p-toast>
      <p-card header="Welcome Back" subheader="Sign in to continue" styleClass="w-full max-w-md bg-slate-800 text-slate-100 border-none shadow-xl">
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="flex flex-col gap-4 mt-4">
          <div class="flex flex-col gap-2">
            <label for="username" class="text-slate-300">Username</label>
            <input pInputText id="username" formControlName="username" class="w-full" />
          </div>
          
          <div class="flex flex-col gap-2">
            <label for="password" class="text-slate-300">Password</label>
            <p-password id="password" formControlName="password" [feedback]="false" styleClass="w-full" inputStyleClass="w-full"></p-password>
          </div>

          <p-button label="Sign In" type="submit" [loading]="loading" styleClass="w-full mt-2"></p-button>
          
          <div class="text-center mt-4 text-sm text-slate-400">
            Don't have an account? <a routerLink="/register" class="text-indigo-400 hover:text-indigo-300">Register</a>
          </div>
        </form>
      </p-card>
    </div>
  `
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private messageService = inject(MessageService);

  loading = false;

  loginForm = this.fb.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  onSubmit() {
    if (this.loginForm.valid) {
      this.loading = true;
      // @ts-ignore
      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          this.router.navigate(['/projects']);
        },
        error: (err) => {
          this.loading = false;
          this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error.message || 'Login failed' });
        }
      });
    }
  }
}
