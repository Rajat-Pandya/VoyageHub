import { Component, computed, inject, signal, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Auth } from './core/services/auth';
import { TravelRequestService } from './core/services/travel-request.service';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatListModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  @ViewChild('sidenav') sidenav?: MatSidenav;
  private breakpointObserver = inject(BreakpointObserver);
  readonly authService = inject(Auth);
  readonly travelService = inject(TravelRequestService);

  isMobile = signal(false);

  readonly isLoggedIn = computed(() => this.authService.isLoggedIn());
  readonly currentUser = computed(() => this.authService.currentUser());
  readonly isBackendConnected = computed(() => this.travelService.isBackendConnected());

  constructor() {
    this.breakpointObserver.observe(['(max-width: 767px)']).subscribe((result) => {
      this.isMobile.set(result.matches);
    });
  }

  onNavClick() {
    if (this.isMobile() && this.sidenav) {
      this.sidenav.close();
    }
  }

  logout() {
    this.authService.logout();
  }
}
