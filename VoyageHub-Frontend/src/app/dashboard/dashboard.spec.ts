import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { vi } from 'vitest';

import { Dashboard } from './dashboard';
import { AuthService } from '../services/auth.service';

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;
  let authService: AuthService;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [provideRouter([])],
    }).compileComponents();

    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show guest message when user is not logged in', () => {
    authService.logout();
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Session Inactive');
  });

  it('should show user profile when user is authenticated', () => {
    authService.loginUser('alex@example.com', 'password123');
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Welcome, Alex Morgan!');
    expect(compiled.textContent).toContain('alex@example.com');
  });

  it('should logout and redirect on logout click', () => {
    authService.loginUser('alex@example.com', 'password123');
    const navSpy = vi.spyOn(router, 'navigate');
    component.logout();

    expect(authService.currentUser()).toBeNull();
    expect(navSpy).toHaveBeenCalledWith(['/login'], { queryParams: { loggedOut: 'true' } });
  });
});
