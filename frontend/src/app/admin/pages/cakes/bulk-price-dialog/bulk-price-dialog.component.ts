import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ApiService, Category } from '../../../../services/api.service';

export interface BulkPriceDialogResult {
  categorySlug: string | null;
  percentChange: number;
}

@Component({
  selector: 'app-bulk-price-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './bulk-price-dialog.component.html',
  styleUrls: ['./bulk-price-dialog.component.scss']
})
export class BulkPriceDialogComponent implements OnInit {
  categories: Category[] = [];
  selectedCategorySlug = '';
  percentChange: number | null = null;

  constructor(
    public dialogRef: MatDialogRef<BulkPriceDialogComponent>,
    private apiService: ApiService
  ) {}

  ngOnInit(): void {
    this.apiService.getCategories().subscribe({
      next: (categories) => this.categories = categories,
      error: () => this.categories = []
    });
  }

  canConfirm(): boolean {
    return this.percentChange !== null
      && this.percentChange > -100
      && this.percentChange !== 0
      && this.percentChange <= 500;
  }

  getPercentError(): string | null {
    if (this.percentChange === null || this.percentChange === 0) return null;
    if (this.percentChange > 500) return 'Максимальное значение — 500%';
    if (this.percentChange <= -100) return 'Минимальное значение — -99%';
    return null;
  }

  onConfirm(): void {
    if (!this.canConfirm() || this.percentChange === null) return;
    this.dialogRef.close({
      categorySlug: this.selectedCategorySlug || null,
      percentChange: this.percentChange
    } as BulkPriceDialogResult);
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
