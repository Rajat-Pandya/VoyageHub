import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VoyageService, AuditLogItem } from '../../services/voyage.service';

@Component({
  selector: 'app-audit-logs',
  imports: [CommonModule, DatePipe, FormsModule],
  templateUrl: './audit-logs.html',
  styleUrl: './audit-logs.css'
})
export class AuditLogs implements OnInit {
  private readonly voyageService = inject(VoyageService);

  readonly logs = signal<AuditLogItem[]>([]);
  readonly isLoading = signal(true);

  selectedSeverity = 'ALL';
  selectedEntityType = 'ALL';

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading.set(true);
    this.voyageService.getAuditLogs(this.selectedSeverity, this.selectedEntityType).subscribe({
      next: (data) => {
        this.logs.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  downloadLogs(): void {
    const list = this.logs();
    let csv = 'Timestamp,EntityType,Action,Actor,ChangeDetail,Severity\n';
    list.forEach(l => {
      csv += `"${l.timestamp}","${l.entityType}","${l.action}","${l.actor}","${l.changeDetail}","${l.severity}"\n`;
    });
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'VoyageHub_Audit_Logs.csv';
    a.click();
  }
}
