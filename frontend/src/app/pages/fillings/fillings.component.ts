import { Component, OnInit, AfterViewInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { ApiService, Filling } from '../../services/api.service';
import { ImagePreviewDialogComponent } from '../../shared/image-preview-dialog/image-preview-dialog.component';

@Component({
  selector: 'app-fillings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './fillings.component.html',
  styleUrls: ['./fillings.component.scss']
})
export class FillingsComponent implements OnInit, AfterViewInit {
  fillings: Filling[] = [];
  isLoading = true;
  error: string | null = null;

  placeholderImage = '/images/placeholder-cake.jpg';

  constructor(public apiService: ApiService, private dialog: MatDialog) { }

  ngOnInit(): void {
    this.loadFillings();
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.checkVisibility(), 100);
  }

  @HostListener('window:scroll', ['$event'])
  onScroll(): void {
    this.checkVisibility();
  }

  checkVisibility(): void {
    const elements = document.querySelectorAll('.animate-on-scroll');
    const windowHeight = window.innerHeight;
    const triggerPoint = 100;

    elements.forEach(element => {
      const rect = element.getBoundingClientRect();
      if (rect.top < windowHeight - triggerPoint) {
        element.classList.add('visible');
      }
    });
  }

  loadFillings(): void {
    this.isLoading = true;
    this.apiService.getAvailableFillings().subscribe({
      next: (data: Filling[]) => {
        this.fillings = data;
        this.isLoading = false;
        setTimeout(() => this.checkVisibility(), 100);
      },
      error: (err: any) => {
        console.error('Error loading fillings:', err);
        this.error = 'Не удалось загрузить начинки';
        this.isLoading = false;
      }
    });
  }

  getFillingImageUrl(imageName: string): string {
    return this.apiService.getFillingImageUrl(imageName);
  }

  handleImageError(event: any): void {
    if (event.target.src !== this.placeholderImage) {
      event.target.src = this.placeholderImage;
      event.target.onerror = null;
    }
  }

  openImagePreview(imageUrl: string, alt: string): void {
    if (!imageUrl) return;
    this.dialog.open(ImagePreviewDialogComponent, {
      data: { imageUrl, alt },
      panelClass: 'image-preview-dialog'
    });
  }
}
