import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '../navbar/navbar.component';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-main-layout',
    standalone: true,
    imports: [CommonModule, RouterOutlet, NavbarComponent, ToastModule],
    template: `
    <div class="min-h-screen bg-gray-950 flex flex-col">
      <app-navbar></app-navbar>
      <main class="flex-1 relative">
        <router-outlet></router-outlet>
      </main>
      <p-toast position="bottom-right"></p-toast>
    </div>
  `,
    styles: []
})
export class MainLayoutComponent { }
