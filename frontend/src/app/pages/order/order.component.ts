import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, Cake, Filling, OrderRequest } from '../../services/api.service';

@Component({
  selector: 'app-order',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './order.component.html',
  styleUrls: ['./order.component.scss']
})
export class OrderComponent implements OnInit {
  cakes: Cake[] = [];
  fillings: Filling[] = [];
  selectedCakeId?: number;
  selectedFillingId?: number;
  customOrder = false;
  orderData: OrderRequest = {
    customerName: '',
    customerPhone: '',
    customerEmail: '',
    customCakeDescription: '',
    weight: 2,
    deliveryDate: undefined,
    deliveryAddress: '',
    comment: ''
  };
  isLoading = false;
  submitted = false;
  successMessage = '';
  errorMessage = '';

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    Promise.all([
      this.apiService.getFeaturedCakes().toPromise(),
      this.apiService.getAvailableFillings().toPromise()
    ]).then(([cakes, fillings]) => {
      this.cakes = cakes || [];
      this.fillings = fillings || [];
      this.isLoading = false;
    }).catch(error => {
      console.error('Error loading data:', error);
      this.errorMessage = 'Не удалось загрузить данные';
      this.isLoading = false;
    });
  }

  ngAfterViewInit() {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
          }
        });
      },
      { threshold: 0.1 }
    );

    const elements = document.querySelectorAll('.animate-on-scroll');
    elements.forEach(el => observer.observe(el));
  }

  onSubmit(): void {
    if (!this.orderData.customerName || !this.orderData.customerPhone) {
      this.errorMessage = 'Пожалуйста, заполните имя и телефон';
      return;
    }

    if (!this.customOrder && !this.selectedCakeId) {
      this.errorMessage = 'Пожалуйста, выберите торт';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const order: OrderRequest = {
      ...this.orderData,
      cakeId: !this.customOrder ? this.selectedCakeId : undefined,
      fillingId: this.selectedFillingId
    };

    this.apiService.createOrder(order).subscribe({
      next: () => {
        this.submitted = true;
        this.successMessage = 'Ваш заказ успешно отправлен! Мы свяжемся с вами в ближайшее время.';
        this.isLoading = false;
        this.resetForm();
      },
      error: (error) => {
        this.errorMessage = 'Произошла ошибка при отправке заказа. Пожалуйста, попробуйте позже.';
        this.isLoading = false;
        console.error('Order error:', error);
      }
    });
  }

  resetForm(): void {
    setTimeout(() => {
      this.submitted = false;
      this.successMessage = '';
      this.orderData = {
        customerName: '',
        customerPhone: '',
        customerEmail: '',
        customCakeDescription: '',
        weight: 2,
        deliveryDate: undefined,
        deliveryAddress: '',
        comment: ''
      };
      this.selectedCakeId = undefined;
      this.selectedFillingId = undefined;
      this.customOrder = false;
    }, 5000);
  }
}
