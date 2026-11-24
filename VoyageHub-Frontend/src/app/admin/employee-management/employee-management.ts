import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  VoyageService,
  EmployeeItem,
  CreateEmployeePayload,
  UpdateEmployeePayload,
  BatchUploadResult
} from '../../services/voyage.service';

@Component({
  selector: 'app-employee-management',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './employee-management.html',
  styleUrl: './employee-management.css'
})
export class EmployeeManagement implements OnInit {
  private readonly voyageService = inject(VoyageService);

  readonly employees = signal<EmployeeItem[]>([]);
  readonly isLoading = signal(false);
  readonly toastMessage = signal<string | null>(null);

  // Filters
  readonly searchTerm = signal('');
  readonly selectedDepartment = signal('All');
  readonly selectedRole = signal('All');

  // Add / Edit Modal
  readonly showAddEditModal = signal(false);
  readonly isEditMode = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly isSaving = signal(false);

  employeeForm = {
    name: '',
    email: '',
    role: 'Employee',
    departmentName: '',
    managerId: '',
    password: '',
    avatarUrl: ''
  };

  // Delete Modal
  readonly showDeleteModal = signal(false);
  readonly employeeToDelete = signal<EmployeeItem | null>(null);
  readonly isDeleting = signal(false);

  // Batch CSV Upload Modal
  readonly showBatchModal = signal(false);
  readonly selectedCsvFile = signal<File | null>(null);
  readonly isUploadingBatch = signal(false);
  readonly batchResult = signal<BatchUploadResult | null>(null);

  // Computed filter options
  readonly departments = computed(() => {
    const set = new Set<string>();
    for (const emp of this.employees()) {
      if (emp.departmentName && emp.departmentName.trim()) {
        set.add(emp.departmentName.trim());
      }
    }
    return Array.from(set).sort((a, b) => a.localeCompare(b));
  });

  readonly managerOptions = computed(() => {
    return this.employees().filter(
      e => (e.role === 'Manager' || e.role === 'Admin') && e.id !== this.editingId()
    );
  });

  readonly filteredEmployees = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    const dept = this.selectedDepartment();
    const role = this.selectedRole();

    return this.employees().filter(emp => {
      const matchSearch =
        !term ||
        emp.name.toLowerCase().includes(term) ||
        emp.email.toLowerCase().includes(term) ||
        (emp.departmentName && emp.departmentName.toLowerCase().includes(term));

      const matchDept =
        dept === 'All' ||
        (emp.departmentName && emp.departmentName.toLowerCase() === dept.toLowerCase());

      const matchRole =
        role === 'All' || emp.role.toLowerCase() === role.toLowerCase();

      return matchSearch && matchDept && matchRole;
    });
  });

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.isLoading.set(true);
    this.voyageService.getEmployees().subscribe({
      next: (data) => {
        this.employees.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.showToast('Failed to load employee directory.');
      }
    });
  }

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => {
      this.toastMessage.set(null);
    }, 4000);
  }

  // --- Add / Edit Handlers ---
  openAddModal(): void {
    this.isEditMode.set(false);
    this.editingId.set(null);
    this.employeeForm = {
      name: '',
      email: '',
      role: 'Employee',
      departmentName: '',
      managerId: '',
      password: '',
      avatarUrl: ''
    };
    this.showAddEditModal.set(true);
  }

  openEditModal(emp: EmployeeItem): void {
    this.isEditMode.set(true);
    this.editingId.set(emp.id);
    this.employeeForm = {
      name: emp.name,
      email: emp.email,
      role: emp.role,
      departmentName: emp.departmentName || '',
      managerId: emp.managerId || '',
      password: '',
      avatarUrl: emp.avatarUrl || ''
    };
    this.showAddEditModal.set(true);
  }

  closeAddEditModal(): void {
    this.showAddEditModal.set(false);
    this.editingId.set(null);
  }

  saveEmployee(): void {
    if (!this.employeeForm.name.trim()) {
      this.showToast('Please enter the employee full name.');
      return;
    }
    if (!this.employeeForm.email.trim() || !this.employeeForm.email.includes('@')) {
      this.showToast('Please enter a valid corporate email address.');
      return;
    }

    this.isSaving.set(true);

    if (this.isEditMode()) {
      const id = this.editingId()!;
      const payload: UpdateEmployeePayload = {
        name: this.employeeForm.name.trim(),
        email: this.employeeForm.email.trim(),
        role: this.employeeForm.role,
        departmentName: this.employeeForm.departmentName.trim() || undefined,
        managerId: this.employeeForm.managerId || undefined,
        password: this.employeeForm.password.trim() || undefined,
        avatarUrl: this.employeeForm.avatarUrl.trim() || undefined
      };

      this.voyageService.updateEmployee(id, payload).subscribe({
        next: (updated) => {
          this.isSaving.set(false);
          this.closeAddEditModal();
          this.loadEmployees();
          this.showToast(`Updated employee details for ${updated.name}.`);
        },
        error: (err) => {
          this.isSaving.set(false);
          const msg = err.error?.message || 'Failed to update employee.';
          this.showToast(msg);
        }
      });
    } else {
      const payload: CreateEmployeePayload = {
        name: this.employeeForm.name.trim(),
        email: this.employeeForm.email.trim(),
        role: this.employeeForm.role,
        departmentName: this.employeeForm.departmentName.trim() || undefined,
        managerId: this.employeeForm.managerId || undefined,
        password: this.employeeForm.password.trim() || undefined,
        avatarUrl: this.employeeForm.avatarUrl.trim() || undefined
      };

      this.voyageService.createEmployee(payload).subscribe({
        next: (created) => {
          this.isSaving.set(false);
          this.closeAddEditModal();
          this.loadEmployees();
          this.showToast(`Created employee account for ${created.name}.`);
        },
        error: (err) => {
          this.isSaving.set(false);
          const msg = err.error?.message || 'Failed to create employee.';
          this.showToast(msg);
        }
      });
    }
  }

  // --- Delete Handlers ---
  openDeleteModal(emp: EmployeeItem): void {
    this.employeeToDelete.set(emp);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.employeeToDelete.set(null);
  }

  confirmDelete(): void {
    const emp = this.employeeToDelete();
    if (!emp) return;

    this.isDeleting.set(true);
    this.voyageService.deleteEmployee(emp.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.closeDeleteModal();
        this.loadEmployees();
        this.showToast(`Employee ${emp.name} has been deleted.`);
      },
      error: (err) => {
        this.isDeleting.set(false);
        const msg = err.error?.message || 'Failed to delete employee.';
        this.showToast(msg);
      }
    });
  }

  // --- CSV Batch Upload Handlers ---
  openBatchModal(): void {
    this.selectedCsvFile.set(null);
    this.batchResult.set(null);
    this.showBatchModal.set(true);
  }

  closeBatchModal(): void {
    this.showBatchModal.set(false);
    this.selectedCsvFile.set(null);
    this.batchResult.set(null);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (file.name.toLowerCase().endsWith('.csv')) {
        this.selectedCsvFile.set(file);
      } else {
        this.showToast('Please select a valid .csv file.');
        input.value = '';
      }
    }
  }

  uploadBatchCsv(): void {
    const file = this.selectedCsvFile();
    if (!file) {
      this.showToast('Please select a CSV file to upload.');
      return;
    }

    this.isUploadingBatch.set(true);
    this.voyageService.batchUploadEmployees(file).subscribe({
      next: (res) => {
        this.isUploadingBatch.set(false);
        this.batchResult.set(res);
        this.loadEmployees();
        this.showToast(`Batch completed: ${res.successCount} added, ${res.failureCount} failed.`);
      },
      error: (err) => {
        this.isUploadingBatch.set(false);
        const msg = err.error?.message || 'Failed to process CSV file.';
        this.showToast(msg);
      }
    });
  }

  downloadTemplate(): void {
    this.voyageService.downloadEmployeeCsvTemplate().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'employees_template.csv';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        // Fallback local download
        const csvContent =
          'Name,Email,Role,Department,Password\n' +
          'Aarav Patel,aarav.patel@voyagehub.in,Employee,Engineering,VoyageHub@2026\n' +
          'Priya Nair,priya.nair@voyagehub.in,Manager,Product Management,VoyageHub@2026\n' +
          'Vikram Mehta,vikram.mehta@voyagehub.in,Finance,Finance & Accounts,VoyageHub@2026\n';
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'employees_template.csv';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
      }
    });
  }
}
