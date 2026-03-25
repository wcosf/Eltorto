import { Component, OnInit, AfterViewInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, Cake, Category, PaginatedResponse } from '../../services/api.service';

@Component({
  selector: 'app-portfolio',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './portfolio.component.html',
  styleUrls: ['./portfolio.component.scss']
})

export class PortfolioComponent implements OnInit, AfterViewInit {
  categories: Category[] = [];
  allCakes: Cake[] = [];
  cakes: Cake[] = [];
  selectedCategory: string = 'all';
  selectedCategoryName: string = 'Все торты';
  isLoading = false;
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

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.loadCategories();
    this.loadAllCakes();
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

  loadCategories(): void {
    this.apiService.getCategories().subscribe({
      next: (categories) => {
        console.log('Categories loaded:', categories);
        if (categories && Array.isArray(categories) && categories.length > 0) {
          this.categories = categories.sort((a, b) => a.sortOrder - b.sortOrder);
        } else {
          this.categories = [];
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.categories = [];
      }
    });
  }

  loadAllCakes(): void {
    this.isLoading = true;
    this.error = null;

    console.log('Loading all cakes for category:', this.selectedCategory);

    if (this.selectedCategory === 'all') {
      this.apiService.getCakesPaged(1, 1000).subscribe({
        next: (response: PaginatedResponse<Cake>) => {
          console.log('All cakes loaded:', response?.items?.length);
          this.allCakes = response?.items || [];
          this.totalItems = this.allCakes.length;
          this.updatePagination();
          this.isLoading = false;
          setTimeout(() => this.checkVisibility(), 100);
        },
        error: (error) => {
          console.error('Error loading all cakes:', error);
          this.error = 'Не удалось загрузить портфолио';
          this.isLoading = false;
        }
      });
    } else {
      this.apiService.getCakesByCategory(this.selectedCategory, 1, 1000).subscribe({
        next: (cakes: Cake[]) => {
          console.log('Category cakes loaded:', cakes?.length);
          this.allCakes = cakes || [];
          this.totalItems = this.allCakes.length;
          this.updatePagination();
          this.isLoading = false;
          setTimeout(() => this.checkVisibility(), 100);
        },
        error: (error) => {
          console.error('Error loading category cakes:', error);
          this.error = 'Не удалось загрузить торты категории';
          this.isLoading = false;
        }
      });
    }
  }

  updatePagination(): void {
    this.totalPages = Math.ceil(this.totalItems / this.pageSize);

    if (this.currentPage > this.totalPages && this.totalPages > 0) {
      this.currentPage = 1;
    }

    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.cakes = this.allCakes.slice(startIndex, endIndex);

    console.log(`Page ${this.currentPage}: showing ${this.cakes.length} of ${this.totalItems} cakes`);
  }

  filterByCategory(categorySlug: string, categoryName: string): void {
    console.log('Filtering by category:', categorySlug, categoryName);
    this.selectedCategory = categorySlug;
    this.selectedCategoryName = categoryName;
    this.currentPage = 1;
    this.loadAllCakes();
  }

  resetFilter(): void {
    this.selectedCategory = 'all';
    this.selectedCategoryName = 'Все торты';
    this.currentPage = 1;
    this.loadAllCakes();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.updatePagination();

    setTimeout(() => {
      const galleryElement = document.querySelector('.portfolio-grid-section');
      if (galleryElement) {
        galleryElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }, 100);
  }

  getPageNumbers(): number[] {
    if (!this.totalPages || this.totalPages <= 0) return [];

    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  getThumbnailUrl(imageUrl: string, thumbnailUrl: string): string {
    if (thumbnailUrl) {
      return `${this.imagePaths.portfolio}${thumbnailUrl}`;
    }
    if (imageUrl) {
      return `${this.imagePaths.portfolio}${imageUrl}`;
    }
    return this.imagePaths.placeholder;
  }

  handleImageError(event: any): void {
    if (event.target.src === this.imagePaths.placeholder) {
      return;
    }
    event.target.src = this.imagePaths.placeholder;
    event.target.onerror = null;
  }

  trackById(index: number, cake: Cake): number {
    return cake.id;
  }

  getCategoryDescription(): string {
    if (this.selectedCategory === 'all') {
      return `Всего ${this.totalItems} тортов в нашем портфолио`;
    }
    const category = this.categories.find(c => c.slug === this.selectedCategory);
    return category?.description || `Торты в категории "${this.selectedCategoryName}"`;
  }
}
