import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
      <div class="container-fluid">
        <a class="navbar-brand" routerLink="/">Tax Readiness Checker</a>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav me-auto" *ngIf="authService.isAuthenticated()">
            <li class="nav-item">
              <a class="nav-link" routerLink="/tax-return" routerLinkActive="active">Validate Return</a>
            </li>
          </ul>
          <div class="d-flex align-items-center" *ngIf="authService.isAuthenticated()">
            <span class="text-light me-3">{{ authService.currentUserValue?.email }}</span>
            <button class="btn btn-outline-light btn-sm" (click)="logout()">Logout</button>
          </div>
        </div>
      </div>
    </nav>
    <main>
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    main {
      padding: 20px 0;
    }
  `]
})
export class AppComponent {
  constructor(public authService: AuthService, private router: Router) {}

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
