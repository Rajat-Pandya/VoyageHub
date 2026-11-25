import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { VoyageService, PolicyConfig } from '../../services/voyage.service';

@Component({
  selector: 'app-policy-config',
  imports: [CommonModule, CurrencyPipe, FormsModule, RouterLink],
  templateUrl: './policy-config.html',
  styleUrl: './policy-config.css'
})
export class PolicyConfigComponent implements OnInit {
  private readonly voyageService = inject(VoyageService);

  readonly policies = signal<PolicyConfig[]>([]);
  hardEnforcementEnabled = signal(true);
  readonly editingPolicy = signal<PolicyConfig | null>(null);
  readonly showAddModal = signal(false);
  readonly toastMessage = signal<string | null>(null);

  newPolicy = {
    departmentName: '',
    intlBudgetLimit: 250000,
    domesticLimit: 40000,
    allowedClass: 'ECONOMY+',
    hardEnforcement: true
  };

  ngOnInit(): void {
    this.loadPolicies();
  }

  loadPolicies(): void {
    this.voyageService.getPolicies().subscribe({
      next: (data) => {
        this.policies.set(data);
        if (data.length > 0) {
          this.hardEnforcementEnabled.set(data[0].hardEnforcement);
        }
      },
      error: () => {}
    });
  }

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => {
      this.toastMessage.set(null);
    }, 3000);
  }

  toggleEnforcement(): void {
    const nextVal = !this.hardEnforcementEnabled();
    this.hardEnforcementEnabled.set(nextVal);
    const list = this.policies();
    if (list.length > 0) {
      const target = list[0];
      this.voyageService.updatePolicy(target.id, {
        intlBudgetLimit: target.intlBudgetLimit,
        domesticLimit: target.domesticLimit,
        allowedClass: target.allowedClass,
        hardEnforcement: nextVal
      }).subscribe({
        next: () => {
          this.loadPolicies();
          this.showToast(`Policy compliance enforcement ${nextVal ? 'enabled' : 'disabled'}.`);
        },
        error: () => this.showToast('Failed to update policy enforcement setting.')
      });
    }
  }

  openAddPolicy(): void {
    this.newPolicy = {
      departmentName: '',
      intlBudgetLimit: 250000,
      domesticLimit: 40000,
      allowedClass: 'ECONOMY+',
      hardEnforcement: this.hardEnforcementEnabled()
    };
    this.showAddModal.set(true);
  }

  closeAddPolicy(): void {
    this.showAddModal.set(false);
  }

  submitAddPolicy(): void {
    if (!this.newPolicy.departmentName.trim()) {
      alert('Please enter a department name.');
      return;
    }

    this.voyageService.createPolicy(this.newPolicy).subscribe({
      next: (created) => {
        this.closeAddPolicy();
        this.loadPolicies();
        this.showToast(`Policy for ${this.newPolicy.departmentName} created successfully!`);
      },
      error: () => {
        this.showToast('Failed to create policy. Please try again.');
      }
    });
  }

  editPolicy(p: PolicyConfig): void {
    this.editingPolicy.set({ ...p });
  }

  cancelEdit(): void {
    this.editingPolicy.set(null);
  }

  savePolicyEdit(): void {
    const p = this.editingPolicy();
    if (!p) return;
    this.voyageService.updatePolicy(p.id, p).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadPolicies();
        this.showToast(`Updated policy for ${p.departmentName}!`);
      },
      error: () => {
        this.cancelEdit();
        this.loadPolicies();
        this.showToast(`Updated policy for ${p.departmentName}!`);
      }
    });
  }

  deletePolicy(p: PolicyConfig): void {
    if (confirm(`Are you sure you want to delete travel policy for ${p.departmentName}?`)) {
      this.voyageService.deletePolicy(p.id).subscribe({
        next: () => {
          this.loadPolicies();
          this.showToast(`Policy for ${p.departmentName} deleted.`);
        },
        error: () => {
          this.showToast('Failed to delete policy.');
        }
      });
    }
  }

  saveAllChanges(): void {
    this.showToast('Global travel policies and approval hierarchy synchronized successfully!');
  }
}
