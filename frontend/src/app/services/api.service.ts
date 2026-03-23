import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Cake {
  id: number;
  name: string;
  imageUrl: string;
  thumbnailUrl: string;
  categorySlug: string;
  subCategory?: string;
  isFeatured: boolean;
  description?: string;
  fillingId?: number;
}

export interface Filling {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  hasCrossSection: boolean;
}

export interface Category {
  id: number;
  slug: string;
  name: string;
  description?: string;
  sortOrder: number;
}

export interface Testimonial {
  id: number;
  date: Date;
  author: string;
  email?: string;
  text: string;
  response?: string;
  isApproved: boolean;
}

export interface OrderRequest {
  customerName: string;
  customerPhone: string;
  customerEmail?: string;
  cakeId?: number;
  customCakeDescription?: string;
  fillingId?: number;
  weight?: number;
  deliveryDate?: Date;
  deliveryAddress?: string;
  comment?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = '/api';

  constructor(private http: HttpClient) { }

  // Categories
  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.apiUrl}/categories`);
  }

  // Cakes with pagination
  getCakesPaged(page: number = 1, pageSize: number = 12, category?: string): Observable<PaginatedResponse<Cake>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (category && category !== 'all') {
      params = params.set('category', category);
    }

    return this.http.get<PaginatedResponse<Cake>>(`${this.apiUrl}/cakes/paged`, { params });
  }

  // Featured cakes
  getFeaturedCakes(): Observable<Cake[]> {
    return this.http.get<Cake[]>(`${this.apiUrl}/cakes/featured`);
  }

  // Cakes by category with pagination
  getCakesByCategory(categorySlug: string, page: number = 1, pageSize: number = 12): Observable<PaginatedResponse<Cake>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<Cake>>(`${this.apiUrl}/cakes/by-category/${categorySlug}`, { params });
  }

  // Fillings
  getAvailableFillings(): Observable<Filling[]> {
    return this.http.get<Filling[]>(`${this.apiUrl}/fillings/available`);
  }

  // Testimonials with pagination
  getApprovedTestimonials(): Observable<Testimonial[]> {
    return this.http.get<Testimonial[]>(`${this.apiUrl}/testimonials/approved`);
  }

  getTestimonialsPaged(page: number = 1, pageSize: number = 6): Observable<PaginatedResponse<Testimonial>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<Testimonial>>(`${this.apiUrl}/testimonials/paged/approved`, { params });
  }

  getLatestTestimonials(count: number = 3): Observable<Testimonial[]> {
    return this.http.get<Testimonial[]>(`${this.apiUrl}/testimonials/latest?count=${count}`);
  }

  createTestimonial(testimonial: Partial<Testimonial>): Observable<Testimonial> {
    return this.http.post<Testimonial>(`${this.apiUrl}/testimonials`, testimonial);
  }

  // Orders
  createOrder(order: OrderRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/orders`, order);
  }

  // Slider
  getSliderItems(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/slider`);
  }

  // Contacts
  getContacts(): Observable<any> {
    return this.http.get(`${this.apiUrl}/contacts`);
  }
}
