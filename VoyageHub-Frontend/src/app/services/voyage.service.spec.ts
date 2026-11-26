import '@angular/compiler';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { VoyageService } from './voyage.service';

describe('VoyageService', () => {
  let service: VoyageService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), VoyageService]
    });
    service = TestBed.inject(VoyageService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('calls my-policy and available policy departments endpoints', () => {
    service.getMyPolicy().subscribe();
    const r1 = httpMock.expectOne((req) => req.url.endsWith('/policies/my-policy'));
    expect(r1.request.method).toBe('GET');
    r1.flush({ id: '1' });

    service.getAvailablePolicyDepartments().subscribe();
    const r2 = httpMock.expectOne((req) => req.url.endsWith('/policies/departments'));
    expect(r2.request.method).toBe('GET');
    r2.flush(['IT']);
  });

  it('calls expense review and my-expenses endpoints with query params', () => {
    service.getPendingExpenses('Meals', true, 'Reimbursed').subscribe();
    const r1 = httpMock.expectOne((req) => req.url.endsWith('/expenses/pending'));
    expect(r1.request.params.get('category')).toBe('Meals');
    expect(r1.request.params.get('highPriorityOnly')).toBe('true');
    expect(r1.request.params.get('status')).toBe('Reimbursed');
    r1.flush({ totalPendingAmount: 0, pendingClaims: [] });

    service.getMyExpenses().subscribe();
    const r2 = httpMock.expectOne((req) => req.url.endsWith('/expenses/my-expenses'));
    expect(r2.request.method).toBe('GET');
    r2.flush([]);
  });

  it('calls createRequest, addSegment, addStay, submitExpense, and closeRequest', () => {
    service.createRequest({ destination: 'Pune', isInternational: false }).subscribe();
    const r1 = httpMock.expectOne((req) => req.url.endsWith('/travelrequests'));
    expect(r1.request.method).toBe('POST');
    r1.flush({ id: 'req-1' });

    service.addSegment('req-1', { mode: 'Flight' }).subscribe();
    const r2 = httpMock.expectOne((req) => req.url.endsWith('/travelrequests/req-1/segments'));
    expect(r2.request.method).toBe('POST');
    r2.flush({});

    service.addStay('req-1', { hotelName: 'Taj' }).subscribe();
    const r3 = httpMock.expectOne((req) => req.url.endsWith('/travelrequests/req-1/stays'));
    expect(r3.request.method).toBe('POST');
    r3.flush({});

    service.submitExpense({ requestId: 'req-1', category: 'Meals', amount: 500 }).subscribe();
    const r4 = httpMock.expectOne((req) => req.url.endsWith('/expenses'));
    expect(r4.request.method).toBe('POST');
    r4.flush({});

    service.closeRequest('req-1').subscribe();
    const r5 = httpMock.expectOne((req) => req.url.endsWith('/travelrequests/req-1/close'));
    expect(r5.request.method).toBe('POST');
    r5.flush({});
  });
});
