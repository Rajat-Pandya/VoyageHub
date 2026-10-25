import { Component, inject, signal, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { BreakpointObserver } from '@angular/cdk/layout';

@Component({
  selector: 'app-root',
  imports: [
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

  isMobile = signal(false);

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
}

