import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

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

export type OrderStatus = 'New' | 'Processing' | 'Completed' | 'Cancelled';

export interface OrderDto {
  id: number;
  createdAt: string;
  customerName: string;
  customerPhone: string;
  customerEmail?: string;
  cakeId?: number;
  cakeName?: string;
  cakeImageUrl?: string;
  customCakeDescription?: string;
  fillingId?: number;
  fillingName?: string;
  weight?: number;
  deliveryDate?: string;
  deliveryAddress?: string;
  status: OrderStatus;
  comment?: string;
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

export interface ContactSettings {
  id: number;
  phone: string;
  additionalPhone?: string;
  email: string;
  address?: string;
  mapUrl?: string;
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
  getCakesPaged(page: number = 1, pageSize: number = 12, category?: string, search?: string): Observable<PaginatedResponse<Cake>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (category && category !== 'all') {
      params = params.set('category', category);
    }
    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<PaginatedResponse<Cake>>(`${this.apiUrl}/cakes/paged`, { params });
  }

  // Featured cakes
  getFeaturedCakes(): Observable<Cake[]> {
    return this.http.get<Cake[]>(`${this.apiUrl}/cakes/featured`);
  }

  // Cakes by category with pagination
  getCakesByCategory(categorySlug: string, page: number = 1, pageSize: number = 12): Observable<Cake[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<Cake[]>(`${this.apiUrl}/cakes/by-category/${categorySlug}`, { params });
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

  // Получить один отзыв по ID
  getTestimonialById(id: number): Observable<Testimonial> {
    return this.http.get<Testimonial>(`${this.apiUrl}/testimonials/${id}`);
  }

  // ===== ORDER CRUD =====

  createOrder(order: OrderRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/orders`, order);
  }

  getOrdersPaged(page: number = 1, pageSize: number = 20, status?: string): Observable<PaginatedResponse<OrderDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<PaginatedResponse<OrderDto>>(`${this.apiUrl}/orders/admin/paged`, { params });
  }

  updateOrderStatus(id: number, status: OrderStatus): Observable<OrderDto> {
    return this.http.patch<OrderDto>(`${this.apiUrl}/orders/${id}/status`, { status });
  }

  deleteOrder(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/orders/${id}`);
  }

  updateOrder(id: number, order: Partial<OrderRequest>): Observable<OrderDto> {
    return this.http.put<OrderDto>(`${this.apiUrl}/orders/${id}`, order);
  }

  getOrderById(id: number): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${this.apiUrl}/orders/${id}`);
  }

  // Slider
  getSliderItems(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/slider`);
  }

  // Contacts
  getContacts(): Observable<any> {
    return this.http.get(`${this.apiUrl}/contacts`);
  }

  // ===== CATEGORIES CRUD =====

  // Create
  createCategory(category: Partial<Category>): Observable<Category> {
    return this.http.post<Category>(`${this.apiUrl}/categories`, category);
  }

  // Update
  updateCategory(id: number, category: Partial<Category>): Observable<Category> {
    return this.http.put<Category>(`${this.apiUrl}/categories/${id}`, category);
  }

  // Delete
  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/categories/${id}`);
  }

  // Get by ID
  getCategoryById(id: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/categories/${id}`);
  }

  // ===== FILLINGS CRUD =====

  // Get all
  getFillings(page: number = 1, pageSize: number = 10, search?: string): Observable<PaginatedResponse<Filling>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<PaginatedResponse<Filling>>(`${this.apiUrl}/fillings`, { params });
  }

  // Create
  createFilling(filling: Partial<Filling>): Observable<Filling> {
    return this.http.post<Filling>(`${this.apiUrl}/fillings`, filling);
  }

  // Update
  updateFilling(id: number, filling: Partial<Filling>): Observable<Filling> {
    return this.http.put<Filling>(`${this.apiUrl}/fillings/${id}`, filling);
  }

  // Delete
  deleteFilling(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/fillings/${id}`);
  }

  // Upload image
  uploadFillingImage(file: File): Observable<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ imageUrl: string }>(`${this.apiUrl}/fillings/upload`, formData);
  }

  deleteFillingImage(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/fillings/${id}/image`);
  }

  // ===== FILE STORAGE HELPERS =====
  uploadFile(file: File, category: 'fillings' | 'cakes' | 'slider' | 'pages', entityId?: number): Observable<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    let url = `${this.apiUrl}/${category}/upload`;
    if (entityId) {
      url += `?id=${entityId}`;
    }
    return this.http.post<{ imageUrl: string }>(url, formData);
  }

  private getFileUrl(fileName: string | null | undefined, category: 'fillings' | 'cakes' | 'slider' | 'pages'): string {
    if (!fileName) return '';

    if (fileName.startsWith('http://') || fileName.startsWith('https://') || fileName.startsWith('/')) {
      return fileName;
    }

    return `/storage/${category}/${encodeURIComponent(fileName)}`;
  }

  getFillingImageUrl(fileName: string | null | undefined): string {
    return this.getFileUrl(fileName, 'fillings');
  }

  getCakeImageUrl(fileName: string | null | undefined): string {
    return this.getFileUrl(fileName, 'cakes');
  }

  getSliderImageUrl(fileName: string | null | undefined): string {
    return this.getFileUrl(fileName, 'slider');
  }

  getPageImageUrl(fileName: string | null | undefined): string {
    return this.getFileUrl(fileName, 'pages');
  }

  // ===== CAKES CRUD =====

  // create
  createCake(cake: Partial<Cake>): Observable<Cake> {
    return this.http.post<Cake>(`${this.apiUrl}/cakes`, cake);
  }

  // update
  updateCake(id: number, cake: Partial<Cake>): Observable<Cake> {
    return this.http.put<Cake>(`${this.apiUrl}/cakes/${id}`, cake);
  }

  // delete
  deleteCake(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/cakes/${id}`);
  }

  // get by id
  getCakeById(id: number): Observable<Cake> {
    return this.http.get<Cake>(`${this.apiUrl}/cakes/${id}`);
  }

  // delete image
  deleteCakeImage(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/cakes/${id}/image`);
  }

  // ===== TESTIMONIALS CRUD =====

  getTestimonialsAdmin(page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<Testimonial>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<PaginatedResponse<any>>(`${this.apiUrl}/testimonials/paged/all`, { params })
      .pipe(
        map(response => ({
          ...response,
          items: response.items.map((item: any) => ({
            id: item.id ?? item.Id,
            author: item.author ?? item.Author,
            text: item.text ?? item.Text,
            response: item.response ?? item.Response,
            isApproved: item.isApproved ?? item.IsApproved,
            date: item.date ?? item.Date,
            email: item.email ?? item.Email,
          }))
        }))
      );
  }

  updateTestimonial(id: number, data: Partial<Testimonial>): Observable<Testimonial> {
    return this.http.put<Testimonial>(`${this.apiUrl}/testimonials/${id}`, data);
  }

  approveTestimonial(id: number, isApproved: boolean): Observable<Testimonial> {
    return this.http.patch<Testimonial>(`${this.apiUrl}/testimonials/${id}/approve`, { isApproved });
  }

  deleteTestimonial(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/testimonials/${id}`);
  }

}
