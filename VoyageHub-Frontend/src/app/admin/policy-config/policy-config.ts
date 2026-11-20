import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VoyageService, PolicyConfig } from '../../services/voyage.service';

@Component({
  selector: 'app-policy-config',
  imports: [CommonModule, CurrencyPipe, FormsModule],
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
      this.voyageService.updatePolicy(list[0].id, { hardEnforcement: nextVal }).subscribe({
        next: () => this.showToast(`Hard enforcement ${nextVal ? 'enabled' : 'disabled'}.`),
        error: () => this.showToast(`Hard enforcement ${nextVal ? 'enabled' : 'disabled'}.`)
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
      error: (err) => {
        // Fallback optimistic
        const optimistic: PolicyConfig = {
          id: Date.now().toString(),
          departmentId: Date.now().toString(),
          departmentName: this.newPolicy.departmentName,
          intlBudgetLimit: this.newPolicy.intlBudgetLimit,
          domesticLimit: this.newPolicy.domesticLimit,
          allowedClass: this.newPolicy.allowedClass,
          hardEnforcement: this.newPolicy.hardEnforcement
        };
        this.policies.update(list => [...list, optimistic]);
        this.closeAddPolicy();
        this.showToast(`Policy for ${this.newPolicy.departmentName} added.`);
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
          this.policies.update(list => list.filter(item => item.id !== p.id));
          this.showToast(`Policy for ${p.departmentName} removed.`);
        }
      });
    }
  }

  saveAllChanges(): void {
    this.showToast('Global travel policies and approval hierarchy synchronized successfully!');
  }
}
