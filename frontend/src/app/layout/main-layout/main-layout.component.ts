import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '../navbar/navbar.component';
import { ToastModule } from 'primeng/toast';
import { DocumentationService } from '../../core/services/documentation.service';
import { DocumentationPanelComponent } from '../documentation-panel/documentation-panel.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, NavbarComponent, ToastModule, DocumentationPanelComponent],
  template: `
    <div class="min-h-screen bg-gray-950 flex flex-col">
      <app-navbar></app-navbar>
      <main class="flex-1 relative flex flex-col">
        <router-outlet></router-outlet>
      </main>
      <app-documentation-panel [class.visible]="docService.panelVisible()"></app-documentation-panel>
      <p-toast position="bottom-right"></p-toast>
    </div>
  `,
  styles: []
})
export class MainLayoutComponent {
  docService = inject(DocumentationService);
}
