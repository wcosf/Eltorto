import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, Cake, Category, PaginatedResponse } from '../../services/api.service';

@Component({
  selector: 'app-portfolio',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './portfolio.component.html',
  styleUrls: ['./portfolio.component.scss']
})

export class PortfolioComponent implements OnInit {
  categories: Category[] = [];
  cakes: Cake[] = [];
  selectedCategory: string = 'all';
  isLoading = false;
  isLoadingMore = false;
  error: string | null = null;

  currentPage = 1;
  pageSize = 12;
  totalPages = 0;
  totalItems = 0;

  imagePaths = {
    portfolio: '/images/portfolio/',
    fillings: '/images/fillings/',
    placeholder: '/images/placeholder-cake.jpg'
  };

  private imageCache = new Map<string, string>();

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.loadCategories();
    this.loadCakes();
  }

  loadCategories(): void {
    this.apiService.getCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadCakes(): void {
    this.isLoading = true;
    this.error = null;

    const request = this.selectedCategory === 'all'
      ? this.apiService.getCakesPaged(this.currentPage, this.pageSize)
      : this.apiService.getCakesByCategory(this.selectedCategory, this.currentPage, this.pageSize);

    request.subscribe({
      next: (response: PaginatedResponse<Cake>) => {
        this.cakes = response.items;
        this.totalPages = response.totalPages;
        this.totalItems = response.totalCount;
        this.isLoading = false;
        window.scrollTo({ top: 0, behavior: 'smooth' });
      },
      error: (error) => {
        console.error('Error loading cakes:', error);
        this.error = 'Не удалось загрузить портфолио';
        this.isLoading = false;
      }
    });
  }

  filterByCategory(categorySlug: string): void {
    this.selectedCategory = categorySlug;
    this.currentPage = 1;
    this.loadCakes();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadCakes();
  }

  getImageUrl(imageName: string): string {
  if (!imageName) {
    return this.imagePaths.placeholder;
  }

  if (imageName.startsWith('sm-')) {
    const originalName = imageName.replace('sm-', '');
    return `${this.imagePaths.portfolio}${originalName}`;
  }
  return `${this.imagePaths.portfolio}${imageName}`;
}

  handleImageError(event: any): void {
    if (event.target.src === this.imagePaths.placeholder) {
      return;
    }
    event.target.src = this.imagePaths.placeholder;
    event.target.onerror = null;
  }

  openModal(cake: Cake): void {
    alert(`🍰 ${cake.name}\n\n${cake.description || 'Торт ручной работы'}`);
  }

  trackById(index: number, cake: Cake): number {
    return cake.id;
  }

  getThumbnailUrl(imageName: string, thumbnailName: string): string {
  if (thumbnailName) {
    return `${this.imagePaths.portfolio}${thumbnailName}`;
  }
  return this.getImageUrl(imageName);
}

}
