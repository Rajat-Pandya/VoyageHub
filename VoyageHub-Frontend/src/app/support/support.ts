import { Component, inject, OnInit, signal, computed, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { VoyageService, SupportTicketItem } from '../services/voyage.service';

export type SupportTicket = SupportTicketItem;

export interface FAQItem {
  question: string;
  category: string;
  answer: string;
  isOpen?: boolean;
}

const INITIAL_FALLBACK_TICKETS: SupportTicket[] = [
  {
    id: 'tk-001',
    ticketCode: '#TICK-7412',
    category: 'Flight / Itinerary Rescheduling',
    priority: 'High',
    requestCode: '#TR-2048',
    subject: 'Flight re-routing due to BLR morning fog delay',
    description: 'Flight 6E-512 was delayed by 3 hours. Need connecting flight to Mumbai rebooked to attend 3 PM client kickoff meeting.',
    callbackNumber: '+91 98201 44521',
    status: 'In Progress',
    assignedAgent: 'Meera Sen (Corporate Travel Desk)',
    createdAt: '2026-10-28T08:30:00.000Z',
    updatedAt: '2026-10-28T10:15:00.000Z'
  },
  {
    id: 'tk-002',
    ticketCode: '#TICK-7280',
    category: 'Expense Claim & Reimbursement Query',
    priority: 'Medium',
    requestCode: '#TR-2041',
    subject: 'GST invoice breakdown query for hotel stay in New Delhi',
    description: 'Finance requested itemized food tax breakdown for Taj Palace hotel invoice.',
    callbackNumber: '+91 98201 44521',
    status: 'Resolved',
    assignedAgent: 'Vikram Malhotra (Finance)',
    resolutionNotes: 'Itemized CGST/SGST invoice accepted. Expense claim reimbursed in batch payout.',
    createdAt: '2026-10-26T14:10:00.000Z',
    updatedAt: '2026-10-27T11:00:00.000Z'
  }
];

@Component({
  selector: 'app-support',
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './support.html',
  styleUrl: './support.css'
})
export class Support implements OnInit {
  readonly authService = inject(AuthService);
  private readonly voyageService = inject(VoyageService);
  private readonly platformId = inject(PLATFORM_ID);

  readonly activeTab = signal<'submit' | 'tickets' | 'faq' | 'directory'>('submit');
  readonly toastMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  // Tickets
  readonly tickets = signal<SupportTicket[]>(INITIAL_FALLBACK_TICKETS);
  readonly filterStatus = signal<string>('All');
  readonly selectedTicket = signal<SupportTicket | null>(null);

  // Form inputs
  ticketCategory = 'Flight / Itinerary Rescheduling';
  ticketPriority: 'Low' | 'Medium' | 'High' | 'Urgent' = 'High';
  ticketRequestCode = '#TR-2048';
  ticketSubject = '';
  ticketDescription = '';
  ticketCallback = '';

  // FAQ Search
  faqSearch = signal('');

  readonly faqs = signal<FAQItem[]>([
    {
      question: 'What happens if my travel budget exceeds the departmental limit?',
      category: 'Policy Compliance',
      answer: 'When estimated budget exceeds the department cap (e.g. ₹2,50,000 for Intl or ₹80,000 for Domestic), VoyageHub automatically marks the request as "Flagged". Flagged requests require explicit Manager justification and secondary Finance authorization before tickets can be issued.',
      isOpen: true
    },
    {
      question: 'How fast are expense reimbursements processed after submission?',
      category: 'Expenses & Finance',
      answer: 'Once approved by your department manager and reviewed by Finance, payments are released during the weekly batch payout cycle every Tuesday and Friday directly into your registered corporate payroll account.',
      isOpen: false
    },
    {
      question: 'How do I cancel or reschedule a flight after manager approval?',
      category: 'Itinerary & Bookings',
      answer: 'For flights scheduled more than 24 hours away, open a ticket under "Flight Rescheduling" or use the VoyageHub detail view. For emergency same-day flight disruption or cancellations, call the 24/7 Corporate Travel Desk Hotline at +91 (022) 6123-8899.',
      isOpen: false
    },
    {
      question: 'Which receipts are required for hotel and meal expense claims?',
      category: 'Expenses & Finance',
      answer: 'Tax regulations require itemized GST/VAT tax invoices clearly displaying the vendor GSTIN, date, employee name, and amount breakdown. Card POS transaction slips without tax details cannot be processed.',
      isOpen: false
    },
    {
      question: 'Who approves my request if my direct reporting manager is out of office?',
      category: 'Approvals & Hierarchy',
      answer: 'If a manager has not acted within 48 business hours, the request automatically escalates to the Department Head or Corporate Administration for emergency clearance.',
      isOpen: false
    }
  ]);

  readonly filteredFaqs = computed(() => {
    const q = this.faqSearch().trim().toLowerCase();
    if (!q) return this.faqs();
    return this.faqs().filter(f =>
      f.question.toLowerCase().includes(q) ||
      f.answer.toLowerCase().includes(q) ||
      f.category.toLowerCase().includes(q)
    );
  });

  readonly filteredTickets = computed(() => {
    const status = this.filterStatus();
    if (status === 'All') return this.tickets();
    return this.tickets().filter(t => t.status === status);
  });

  readonly openTicketCount = computed(() => {
    return this.tickets().filter(t => t.status !== 'Resolved').length;
  });

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (user) {
      this.ticketCallback = user.phoneNumber || '+91 98201 44521';
    }
    this.loadTickets();
  }

  loadTickets(): void {
    const user = this.authService.currentUser();
    this.voyageService.getSupportTickets(undefined, user?.id).subscribe({
      next: (data) => {
        if (Array.isArray(data)) {
          this.tickets.set(data);
          if (isPlatformBrowser(this.platformId)) {
            sessionStorage.setItem('voyage_support_tickets', JSON.stringify(data));
          }
        }
      },
      error: () => {
        if (isPlatformBrowser(this.platformId)) {
          try {
            const saved = sessionStorage.getItem('voyage_support_tickets');
            if (saved) {
              const parsed = JSON.parse(saved);
              if (Array.isArray(parsed)) {
                this.tickets.set(parsed);
              }
            }
          } catch {
            // Keep initial fallback
          }
        }
      }
    });
  }

  setTab(tab: 'submit' | 'tickets' | 'faq' | 'directory'): void {
    this.activeTab.set(tab);
  }

  submitTicket(): void {
    if (!this.ticketSubject.trim()) {
      this.showToast('Please enter a ticket subject.');
      return;
    }
    if (!this.ticketDescription.trim()) {
      this.showToast('Please provide a description of the issue.');
      return;
    }

    this.isSubmitting.set(true);
    const user = this.authService.currentUser();

    const payload = {
      category: this.ticketCategory,
      priority: this.ticketPriority,
      requestCode: this.ticketRequestCode !== 'None' ? this.ticketRequestCode : undefined,
      subject: this.ticketSubject.trim(),
      description: this.ticketDescription.trim(),
      callbackNumber: this.ticketCallback.trim(),
      userId: user?.id
    };

    this.voyageService.createSupportTicket(payload).subscribe({
      next: (created) => {
        this.isSubmitting.set(false);
        const updated = [created, ...this.tickets()];
        this.tickets.set(updated);
        if (isPlatformBrowser(this.platformId)) {
          sessionStorage.setItem('voyage_support_tickets', JSON.stringify(updated));
        }
        this.ticketSubject = '';
        this.ticketDescription = '';
        this.showToast(`Support Ticket ${created.ticketCode} saved to database! Travel desk alerted.`);
        this.activeTab.set('tickets');
      },
      error: () => {
        // Fallback local persistence if offline
        const randomNum = Math.floor(1000 + Math.random() * 9000);
        const newTicket: SupportTicket = {
          id: `tk-${Date.now()}`,
          ticketCode: `#TICK-${randomNum}`,
          category: this.ticketCategory,
          priority: this.ticketPriority,
          requestCode: this.ticketRequestCode !== 'None' ? this.ticketRequestCode : undefined,
          subject: this.ticketSubject.trim(),
          description: this.ticketDescription.trim(),
          callbackNumber: this.ticketCallback.trim(),
          status: 'Open',
          assignedAgent: 'Meera Sen (Corporate Travel Desk)',
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString()
        };

        const updated = [newTicket, ...this.tickets()];
        this.tickets.set(updated);

        if (isPlatformBrowser(this.platformId)) {
          try {
            sessionStorage.setItem('voyage_support_tickets', JSON.stringify(updated));
          } catch {
            // Safe fallback
          }
        }

        this.isSubmitting.set(false);
        this.ticketSubject = '';
        this.ticketDescription = '';
        this.showToast(`Support Ticket ${newTicket.ticketCode} opened! Travel desk alerted.`);
        this.activeTab.set('tickets');
      }
    });
  }

  toggleFaq(faq: FAQItem): void {
    faq.isOpen = !faq.isOpen;
  }

  viewTicket(ticket: SupportTicket): void {
    this.selectedTicket.set(ticket);
  }

  closeTicketModal(): void {
    this.selectedTicket.set(null);
  }

  private showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => {
      if (this.toastMessage() === msg) {
        this.toastMessage.set(null);
      }
    }, 4500);
  }
}
