import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type StatusType = 'new' | 'processing' | 'completed' | 'cancelled' | 'approved' | 'pending' | 'active' | 'inactive';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="status-badge" [class]="statusClass">
      {{ label }}
    </span>
  `,
  styleUrls: ['./status-badge.component.scss']
})
export class StatusBadgeComponent {
  @Input() status: StatusType = 'pending';
  @Input() label: string = '';

  get statusClass(): string {
    return this.status;
  }
}
