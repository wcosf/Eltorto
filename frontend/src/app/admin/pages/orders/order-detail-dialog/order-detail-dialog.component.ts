import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { StatusBadgeComponent, StatusType } from '../../../shared/components/status-badge/status-badge.component';
import { ImagePreviewDialogComponent } from '../../../shared/components/image-preview-dialog/image-preview-dialog.component';
import { ApiService, Cake, OrderDto, OrderStatus } from '../../../../services/api.service';

@Component({
  selector: 'app-order-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    StatusBadgeComponent,
  ],
  templateUrl: './order-detail-dialog.component.html',
  styleUrls: ['./order-detail-dialog.component.scss']
})
export class OrderDetailDialogComponent {
  order: OrderDto;
  allCakes: Cake[] = [];

  statusLabels: Record<string, string> = {
    New: 'Новый',
    Processing: 'В обработке',
    Completed: 'Завершён',
    Cancelled: 'Отменён',
  };

  statusBadgeMap: Record<string, StatusType> = {
    New: 'new',
    Processing: 'processing',
    Completed: 'completed',
    Cancelled: 'cancelled',
  };

  constructor(
    public dialogRef: MatDialogRef<OrderDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { order: OrderDto },
    private apiService: ApiService,
    private dialog: MatDialog
  ) {
    this.order = data.order;
    this.loadCakes();
  }

  private loadCakes(): void {
    this.apiService.getCakesPaged(1, 10000).subscribe({
      next: (response) => this.allCakes = response.items,
      error: () => { },
    });
  }

  openCakePreview(): void {
    const url = this.order.cakeImageUrl || this.getCakeImageFallback(this.order.cakeId);
    if (!url) return;
    const fullUrl = this.apiService.getCakeImageUrl(url);
    this.dialog.open(ImagePreviewDialogComponent, {
      data: { imageUrl: fullUrl, alt: this.order.cakeName || 'Фото торта' },
      panelClass: 'image-preview-dialog'
    });
  }

  getCakeImageUrl(url: string | undefined): string {
    return url ? this.apiService.getCakeImageUrl(url) : '';
  }

  getCakeImageFallback(cakeId: number | undefined): string {
    if (!cakeId) return '';
    const cake = this.allCakes.find(c => c.id === cakeId);
    return cake?.imageUrl ? this.apiService.getCakeImageUrl(cake.imageUrl) : '';
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '—';
    const date = new Date(dateStr);
    return date.toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}
