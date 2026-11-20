import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NgForm } from '@angular/forms';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';

import { Login } from './login';
import { AuthService } from '../services/auth.service';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let authService: AuthService;

  const validModel = () => {
    component.email = 'alex@example.com';
    component.password = 'password123';
    component.rememberMe = true;
  };

  const fakeValidForm = () =>
    ({ invalid: false, control: { markAllAsTouched: () => undefined } }) as unknown as NgForm;

  const fakeInvalidForm = () =>
    ({
      invalid: true,
      control: { markAllAsTouched: vi.fn() },
    }) as unknown as NgForm;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [provideRouter([])],
    }).compileComponents();

    authService = TestBed.inject(AuthService);
    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle password visibility signal', () => {
    expect(component.showPassword()).toBe(false);
    component.togglePasswordVisibility();
    expect(component.showPassword()).toBe(true);
    component.togglePasswordVisibility();
    expect(component.showPassword()).toBe(false);
  });

  it('should keep submit disabled when the form is invalid', () => {
    fixture.detectChanges();
    const submit = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      'button[type="submit"]',
    );
    expect(submit?.disabled).toBe(true);
  });

  it('should set a success message on valid login with registered user', () => {
    validModel();
    component.login(fakeValidForm());

    expect(component.successMessage()).toContain('Login successful');
    expect(authService.currentUser()?.email).toBe('alex@example.com');
  });

  it('should set error message when credentials are invalid', () => {
    component.email = 'unknown@example.com';
    component.password = 'password123';
    component.login(fakeValidForm());

    expect(component.errorMessage()).toContain('No account found');
  });

  it('should reject invalid submit and mark fields touched', () => {
    const form = fakeInvalidForm();
    component.login(form);

    expect(form.control.markAllAsTouched).toHaveBeenCalled();
    expect(component.successMessage()).toBeNull();
  });

  it('should reset model state and clear status messages', () => {
    validModel();
    component.login(fakeValidForm());
    expect(component.successMessage()).not.toBeNull();

    const form = { resetForm: vi.fn() } as unknown as NgForm;
    component.resetForm(form);

    expect(form.resetForm).toHaveBeenCalled();
    expect(component.successMessage()).toBeNull();
    expect(component.showPassword()).toBe(false);
  });
});
