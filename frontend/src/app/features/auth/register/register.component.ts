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
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ButtonModule, InputTextModule, PasswordModule, CardModule, ToastModule],
  providers: [MessageService],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-slate-900 p-4">
      <p-toast></p-toast>
      <p-card header="Create Account" subheader="Join the team" styleClass="w-full max-w-md bg-slate-800 text-slate-100 border-none shadow-xl">
        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="flex flex-col gap-4 mt-4">
          <div class="flex flex-col gap-2">
            <label for="username" class="text-slate-300">Username</label>
            <input pInputText id="username" formControlName="username" class="w-full" />
            <small class="text-red-400" *ngIf="registerForm.get('username')?.touched && registerForm.get('username')?.invalid">
              Username is required (min 3 chars)
            </small>
          </div>
          
          <div class="flex flex-col gap-2">
            <label for="password" class="text-slate-300">Password</label>
            <p-password id="password" formControlName="password" [toggleMask]="true" styleClass="w-full" inputStyleClass="w-full"></p-password>
            <small class="text-red-400" *ngIf="registerForm.get('password')?.touched && registerForm.get('password')?.invalid">
              Password is required (min 6 chars)
            </small>
          </div>

          <p-button label="Register" type="submit" [loading]="loading" styleClass="w-full mt-2"></p-button>
          
          <div class="text-center mt-4 text-sm text-slate-400">
            Already have an account? <a routerLink="/login" class="text-indigo-400 hover:text-indigo-300">Sign In</a>
          </div>
        </form>
      </p-card>
    </div>
  `
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private messageService = inject(MessageService);

  loading = false;

  registerForm = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit() {
    if (this.registerForm.valid) {
      this.loading = true;
      // @ts-ignore
      this.authService.register(this.registerForm.value).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Account created!' });
          setTimeout(() => this.router.navigate(['/projects']), 1000);
        },
        error: (err) => {
          this.loading = false;
          this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error.message || 'Registration failed' });
        }
      });
    } else {
      this.registerForm.markAllAsTouched();
    }
  }
}
