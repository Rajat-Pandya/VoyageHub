import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService, getUserInitials } from '../services/auth.service';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.html',
  styleUrl: './layout.css'
})
export class AppLayout {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  readonly getUserInitials = getUserInitials;

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
