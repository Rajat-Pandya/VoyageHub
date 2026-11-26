import '@angular/compiler';
import { describe, it, expect, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpRequest, HttpResponse } from '@angular/common/http';
import { of } from 'rxjs';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  it('adds Authorization and no-cache headers for authenticated GET requests', () => {
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { getToken: vi.fn().mockReturnValue('jwt-123') } }
      ]
    });

    const req = new HttpRequest('GET', '/api/travelrequests');
    let capturedReq: HttpRequest<unknown> | undefined;
    TestBed.runInInjectionContext(() => {
      authInterceptor(req, (r) => {
        capturedReq = r;
        return of(new HttpResponse({ status: 200 }));
      }).subscribe();
    });

    expect(capturedReq?.headers.get('Authorization')).toBe('Bearer jwt-123');
    expect(capturedReq?.headers.get('Cache-Control')).toBe('no-cache, no-store, must-revalidate');
    expect(capturedReq?.headers.get('Pragma')).toBe('no-cache');
  });

  it('adds only Authorization for authenticated POST requests', () => {
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { getToken: vi.fn().mockReturnValue('jwt-123') } }
      ]
    });

    const req = new HttpRequest('POST', '/api/travelrequests', {});
    let capturedReq: HttpRequest<unknown> | undefined;
    TestBed.runInInjectionContext(() => {
      authInterceptor(req, (r) => {
        capturedReq = r;
        return of(new HttpResponse({ status: 200 }));
      }).subscribe();
    });

    expect(capturedReq?.headers.get('Authorization')).toBe('Bearer jwt-123');
    expect(capturedReq?.headers.has('Cache-Control')).toBe(false);
  });

  it('passes through unauthenticated non-GET requests unchanged', () => {
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { getToken: vi.fn().mockReturnValue(null) } }
      ]
    });

    const req = new HttpRequest('POST', '/api/auth/login', {});
    let capturedReq: HttpRequest<unknown> | undefined;
    TestBed.runInInjectionContext(() => {
      authInterceptor(req, (r) => {
        capturedReq = r;
        return of(new HttpResponse({ status: 200 }));
      }).subscribe();
    });

    expect(capturedReq).toBe(req);
  });
});
