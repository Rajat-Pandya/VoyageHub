import { Component, input, computed } from '@angular/core';

@Component({
  selector: 'app-status-chip',
  standalone: true,
  template: `
    <span class="status-chip" [class]="chipClass()">
      {{ label() || status() }}
    </span>
  `,
  styles: [`
    :host {
      display: inline-flex;
    }
  `]
})
export class StatusChipComponent {
  readonly status = input.required<string>();
  readonly label = input<string>();

  readonly chipClass = computed(() => {
    const s = (this.status() || '').toLowerCase().trim();
    if (s.includes('approved') || s.includes('paid') || s === 'active' || s.includes('standard')) {
      return 'chip-approved';
    }
    if (s.includes('reject') || s.includes('flagged') || s === 'inactive' || s.includes('cancel')) {
      return 'chip-rejected';
    }
    if (s.includes('complete') || s.includes('booked')) {
      return 'chip-completed';
    }
    if (s.includes('draft')) {
      return 'chip-draft';
    }
    // Default to review / pending / in-progress
    return 'chip-pending';
  });
}
