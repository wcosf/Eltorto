import { Component, OnInit, AfterViewInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, Filling } from '../../services/api.service';

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

  imagePaths = {
    fillings: '/images/fillings/',
    placeholder: '/images/placeholder-cake.jpg'
  };

  constructor(private apiService: ApiService) { }

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
    if (!imageName) {
      return this.imagePaths.placeholder;
    }
    return `${this.imagePaths.fillings}${imageName}`;
  }

  handleImageError(event: any): void {
    event.target.src = this.imagePaths.placeholder;
  }
}
