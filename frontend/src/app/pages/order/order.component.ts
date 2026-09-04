import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiService, Cake, ContactSettings, Filling, OrderRequest } from '../../services/api.service';
import { RecaptchaService } from '../../services/recaptcha.service';

const ORDER_FORM_STATE_KEY = 'orderFormState';
const ORDER_FORM_TTL_MS = 20 * 60 * 1000;

interface OrderFormState {
  customerName: string;
  customerPhone: string;
  customerEmail?: string;
  customCakeDescription?: string;
  weight: number;
  deliveryDate?: string;
  deliveryAddress?: string;
  comment?: string;
  selectedCakeId?: number;
  selectedFillingId?: number;
  cakeSearch: string;
  fillingSearch: string;
  customOrder: boolean;
  timestamp: number;
}

@Component({
  selector: 'app-order',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './order.component.html',
  styleUrls: ['./order.component.scss']
})
export class OrderComponent implements OnInit, OnDestroy {
  cakes: Cake[] = [];
  fillings: Filling[] = [];
  selectedCakeId?: number;
  selectedFillingId?: number;
  selectedCake: Cake | null = null;
  selectedFilling: Filling | null = null;
  customOrder = true;

  cakeSearch = '';
  fillingSearch = '';
  cakeFiltered: Cake[] = [];
  fillingFiltered: Filling[] = [];
  cakeDropdownOpen = false;
  fillingDropdownOpen = false;
  cakeActiveIndex = -1;
  fillingActiveIndex = -1;
  private closeTimer: number | null = null;
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
  attempted = false;
  touched: Record<string, boolean> = {};
  private readonly phonePattern = /^(\+7|8)\s?\(?\d{3}\)?\s?\d{3}[\s-]?\d{2}[\s-]?\d{2}$/;
  private readonly emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  cakeIdFromUrl?: number;
  private hasSavedState = false;
  datePickerOpen = false;
  shownYear = new Date().getFullYear();
  shownMonth = new Date().getMonth();
  private readonly monthNames = ['январь', 'февраль', 'март', 'апрель', 'май', 'июнь', 'июль', 'август', 'сентябрь', 'октябрь', 'ноябрь', 'декабрь'];
  private readonly weekdayNames = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'];
  private readonly dateDocClickListener = (event: MouseEvent) => this.onDateDocumentClick(event);
  private readonly dateEscKeyListener = (event: KeyboardEvent) => this.onDateEscape(event);
  contacts: ContactSettings | null = null;
  phone = '+7 913 987 2554';
  email = '79139872554@yandex.ru';
  address = 'г. Новосибирск';
  workingHours = '8:00 - 20:00';

  private isFieldVisible(field: string): boolean {
    return this.attempted || !!this.touched[field];
  }

  get showNameError(): boolean {
    return this.isFieldVisible('name')
      && (!this.orderData.customerName || this.orderData.customerName.trim().length < 2);
  }

  get showPhoneError(): boolean {
    return this.isFieldVisible('phone')
      && (!this.orderData.customerPhone || !this.phonePattern.test(this.orderData.customerPhone.trim()));
  }

  get showEmailError(): boolean {
    return this.isFieldVisible('email')
      && !!this.orderData.customerEmail && !this.emailPattern.test(this.orderData.customerEmail.trim());
  }

  get showCakeError(): boolean {
    return this.isFieldVisible('cake') && !this.customOrder && !this.selectedCakeId;
  }

  get selectedCakePreviewUrl(): string {
    const cake = this.selectedCake;
    if (!cake) return '';
    return this.apiService.getCakeImageUrl(cake.imageUrl || cake.thumbnailUrl);
  }

  get selectedFillingPreviewUrl(): string {
    const filling = this.selectedFilling;
    if (!filling) return '';
    return this.apiService.getFillingImageUrl(filling.imageUrl);
  }

  onPreviewError(event: Event): void {
    (event.target as HTMLImageElement).style.display = 'none';
  }

  private dateInputValue(): string | null {
    const value = this.orderData.deliveryDate;
    if (!value) return null;
    if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) return value;
    const date = new Date(value);
    if (isNaN(date.getTime())) return null;
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  }

  get showDateError(): boolean {
    if (!this.isFieldVisible('date')) return false;
    const value = this.dateInputValue();
    if (!value) return true;
    const now = new Date();
    const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
    return value < today;
  }

  get showWeightError(): boolean {
    if (!this.isFieldVisible('weight')) return false;
    const weight = this.orderData.weight;
    if (weight === null || weight === undefined) return false;
    return !(weight > 0 && weight <= 100);
  }

  get showDescriptionError(): boolean {
    return this.isFieldVisible('description')
      && (this.orderData.customCakeDescription?.length ?? 0) > 2000;
  }

  get showCommentError(): boolean {
    return this.isFieldVisible('comment')
      && (this.orderData.comment?.length ?? 0) > 1000;
  }

  get today(): string {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
  }

  get selectedDateText(): string {
    const value = this.dateInputValue();
    if (!value) return 'ДД.ММ.ГГГГ';
    const parts = value.split('-');
    return `${parts[2]}.${parts[1]}.${parts[0]}`;
  }

  get datePickerLabel(): string {
    return `${this.monthNames[this.shownMonth]} ${this.shownYear}`;
  }

  get weekdays(): string[] {
    return this.weekdayNames;
  }

  get dateGridDays(): (number | null)[] {
    const first = new Date(this.shownYear, this.shownMonth, 1);
    let mondayIndex = first.getDay() - 1;
    if (mondayIndex < 0) mondayIndex = 6;
    const daysInMonth = new Date(this.shownYear, this.shownMonth + 1, 0).getDate();
    const cells: (number | null)[] = [];
    for (let i = 0; i < mondayIndex; i++) cells.push(null);
    for (let d = 1; d <= daysInMonth; d++) cells.push(d);
    return cells;
  }

  isDateDisabled(day: number): boolean {
    const dateStr = `${this.shownYear}-${String(this.shownMonth + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
    return dateStr < this.today;
  }

  isSelectedDate(day: number): boolean {
    const value = this.dateInputValue();
    if (!value) return false;
    const target = `${this.shownYear}-${String(this.shownMonth + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
    return value === target;
  }

  toggleDatePicker(): void {
    this.datePickerOpen = !this.datePickerOpen;
    if (this.datePickerOpen) {
      const value = this.dateInputValue();
      if (value) {
        const parts = value.split('-');
        this.shownYear = Number(parts[0]);
        this.shownMonth = Number(parts[1]) - 1;
      } else {
        const now = new Date();
        this.shownYear = now.getFullYear();
        this.shownMonth = now.getMonth();
      }
    }
  }

  openDatePicker(): void {
    if (!this.datePickerOpen) {
      this.datePickerOpen = true;
      const value = this.dateInputValue();
      if (value) {
        const parts = value.split('-');
        this.shownYear = Number(parts[0]);
        this.shownMonth = Number(parts[1]) - 1;
      }
    }
  }

  closeDatePicker(): void {
    this.datePickerOpen = false;
  }

  previousMonth(): void {
    this.shownMonth -= 1;
    if (this.shownMonth < 0) {
      this.shownMonth = 11;
      this.shownYear -= 1;
    }
  }

  nextMonth(): void {
    this.shownMonth += 1;
    if (this.shownMonth > 11) {
      this.shownMonth = 0;
      this.shownYear += 1;
    }
  }

  onDateWheel(event: WheelEvent): void {
    event.preventDefault();
    if (event.deltaY > 0) {
      this.nextMonth();
    } else if (event.deltaY < 0) {
      this.previousMonth();
    }
  }

  selectDate(day: number): void {
    if (this.isDateDisabled(day)) return;
    this.orderData.deliveryDate = `${this.shownYear}-${String(this.shownMonth + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
    this.touched['date'] = true;
    this.closeDatePicker();
    this.saveFormState();
  }

  private onDateDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (target && target.closest('.order-page .date-picker')) return;
    this.closeDatePicker();
  }

  private onDateEscape(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      this.closeDatePicker();
    }
  }

  markTouched(field: string): void {
    this.touched[field] = true;
  }

  private isValid(): boolean {
    return !this.showNameError
      && !this.showPhoneError
      && !this.showEmailError
      && !this.showCakeError
      && !this.showDateError
      && !this.showWeightError
      && !this.showDescriptionError
      && !this.showCommentError;
  }

  constructor(private apiService: ApiService, private route: ActivatedRoute, private recaptchaService: RecaptchaService) { }

  ngOnInit(): void {
    const cakeParam = this.route.snapshot.queryParamMap.get('cake');
    if (cakeParam) {
      const id = Number(cakeParam);
      if (Number.isInteger(id) && id > 0) {
        this.cakeIdFromUrl = id;
      }
    }
    document.addEventListener('click', this.dateDocClickListener);
    document.addEventListener('keydown', this.dateEscKeyListener);
    this.restoreFormState();
    this.loadData();
    this.loadContacts();
  }

  loadData(): void {
    this.isLoading = true;
    Promise.all([
      this.apiService.getAvailableCakes().toPromise(),
      this.apiService.getAvailableFillings().toPromise()
    ]).then(([cakes, fillings]) => {
      this.cakes = cakes || [];
      this.fillings = fillings || [];
      if (this.hasSavedState) {
        this.applySavedSelections();
      } else if (this.cakeIdFromUrl != null && this.cakes.some(c => c.id === this.cakeIdFromUrl)) {
        const cake = this.cakes.find(c => c.id === this.cakeIdFromUrl)!;
        this.selectedCakeId = this.cakeIdFromUrl;
        this.selectedCake = cake;
        this.cakeSearch = cake.name;
        this.onCakeChange(this.cakeIdFromUrl);
      }
      this.isLoading = false;
    }).catch(error => {
      console.error('Error loading data:', error);
      this.errorMessage = 'Не удалось загрузить данные';
      this.isLoading = false;
    });
  }

  onCakeChange(value: number | null | undefined): void {
    this.customOrder = value == null;
  }

  saveFormState(): void {
    const state: OrderFormState = {
      customerName: this.orderData.customerName,
      customerPhone: this.orderData.customerPhone,
      customerEmail: this.orderData.customerEmail,
      customCakeDescription: this.orderData.customCakeDescription,
      weight: typeof this.orderData.weight === 'number' ? this.orderData.weight : 2,
      deliveryDate: this.orderData.deliveryDate,
      deliveryAddress: this.orderData.deliveryAddress,
      comment: this.orderData.comment,
      selectedCakeId: this.selectedCakeId,
      selectedFillingId: this.selectedFillingId,
      cakeSearch: this.cakeSearch,
      fillingSearch: this.fillingSearch,
      customOrder: this.customOrder,
      timestamp: Date.now()
    };
    localStorage.setItem(ORDER_FORM_STATE_KEY, JSON.stringify(state));
  }

  private restoreFormState(): void {
    const raw = localStorage.getItem(ORDER_FORM_STATE_KEY);
    if (!raw) return;
    try {
      const state = JSON.parse(raw) as OrderFormState;
      if (!state || typeof state !== 'object') return;
      if (typeof state.timestamp !== 'number' || Date.now() - state.timestamp > ORDER_FORM_TTL_MS) {
        this.clearFormState();
        return;
      }
      this.hasSavedState = true;
      const numericWeight = Number(state.weight);
      if (!Number.isNaN(numericWeight)) {
        this.orderData.weight = numericWeight;
      }
      if (typeof state.customerName === 'string') this.orderData.customerName = state.customerName;
      if (typeof state.customerPhone === 'string') this.orderData.customerPhone = state.customerPhone;
      if (typeof state.customerEmail === 'string') this.orderData.customerEmail = state.customerEmail;
      if (typeof state.customCakeDescription === 'string') this.orderData.customCakeDescription = state.customCakeDescription;
      if (typeof state.deliveryDate === 'string') this.orderData.deliveryDate = state.deliveryDate;
      if (typeof state.deliveryAddress === 'string') this.orderData.deliveryAddress = state.deliveryAddress;
      if (typeof state.comment === 'string') this.orderData.comment = state.comment;
      if (typeof state.cakeSearch === 'string') this.cakeSearch = state.cakeSearch;
      if (typeof state.fillingSearch === 'string') this.fillingSearch = state.fillingSearch;
      if (typeof state.selectedCakeId === 'number') this.selectedCakeId = state.selectedCakeId;
      if (typeof state.selectedFillingId === 'number') this.selectedFillingId = state.selectedFillingId;
      this.customOrder = typeof state.customOrder === 'boolean' ? state.customOrder : true;
    } catch {
      this.clearFormState();
    }
  }

  private clearFormState(): void {
    localStorage.removeItem(ORDER_FORM_STATE_KEY);
    this.hasSavedState = false;
  }

  private applySavedSelections(): void {
    if (this.selectedCakeId != null) {
      const cake = this.cakes.find(c => c.id === this.selectedCakeId);
      if (cake) {
        this.selectedCakeId = cake.id;
        this.selectedCake = cake;
        this.cakeSearch = cake.name;
        this.onCakeChange(cake.id);
      } else {
        this.selectedCakeId = undefined;
        this.onCakeChange(null);
      }
    }
    if (this.selectedFillingId != null) {
      const filling = this.fillings.find(f => f.id === this.selectedFillingId);
      if (filling) {
        this.selectedFillingId = filling.id;
        this.selectedFilling = filling;
        this.fillingSearch = filling.name;
      } else {
        this.selectedFillingId = undefined;
      }
    }
    this.customOrder = this.selectedCakeId == null;
  }

  ngOnDestroy(): void {
    document.removeEventListener('click', this.dateDocClickListener);
    document.removeEventListener('keydown', this.dateEscKeyListener);
    if (this.closeTimer != null) {
      window.clearTimeout(this.closeTimer);
    }
  }

  filterCakes(): void {
    const q = this.cakeSearch.toLowerCase().trim();
    this.cakeFiltered = q
      ? this.cakes.filter(c => c.name.toLowerCase().includes(q))
      : [...this.cakes];
  }

  filterFillings(): void {
    const q = this.fillingSearch.toLowerCase().trim();
    this.fillingFiltered = q
      ? this.fillings.filter(f => f.name.toLowerCase().includes(q))
      : [...this.fillings];
  }

  onCakeInput(): void {
    this.selectedCakeId = undefined;
    this.selectedCake = null;
    this.customOrder = true;
    this.onCakeChange(null);
    this.cakeActiveIndex = -1;
    this.filterCakes();
    this.cakeDropdownOpen = true;
    this.saveFormState();
  }

  onFillingInput(): void {
    this.selectedFillingId = undefined;
    this.selectedFilling = null;
    this.fillingActiveIndex = -1;
    this.filterFillings();
    this.fillingDropdownOpen = true;
    this.saveFormState();
  }

  onCakeFocus(): void {
    this.filterCakes();
    this.cakeDropdownOpen = true;
  }

  onFillingFocus(): void {
    this.filterFillings();
    this.fillingDropdownOpen = true;
  }

  onCakeBlur(): void {
    this.closeTimer = window.setTimeout(() => { this.cakeDropdownOpen = false; }, 150);
  }

  onFillingBlur(): void {
    this.closeTimer = window.setTimeout(() => { this.fillingDropdownOpen = false; }, 150);
  }

  selectCake(cake: Cake): void {
    this.cakeSearch = cake.name;
    this.selectedCakeId = cake.id;
    this.selectedCake = cake;
    this.customOrder = false;
    this.onCakeChange(cake.id);
    this.cakeActiveIndex = -1;
    this.cakeDropdownOpen = false;
    this.saveFormState();
  }

  selectFilling(filling: Filling): void {
    this.fillingSearch = filling.name;
    this.selectedFillingId = filling.id;
    this.selectedFilling = filling;
    this.fillingActiveIndex = -1;
    this.fillingDropdownOpen = false;
    this.saveFormState();
  }

  onCakeKeyDown(event: KeyboardEvent): void {
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      if (!this.cakeDropdownOpen) this.onCakeFocus();
      this.cakeActiveIndex = Math.min(this.cakeActiveIndex + 1, this.cakeFiltered.length - 1);
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.cakeActiveIndex = Math.max(this.cakeActiveIndex - 1, -1);
    } else if (event.key === 'Enter') {
      if (this.cakeDropdownOpen && this.cakeActiveIndex >= 0 && this.cakeFiltered[this.cakeActiveIndex]) {
        event.preventDefault();
        this.selectCake(this.cakeFiltered[this.cakeActiveIndex]);
      }
    } else if (event.key === 'Escape') {
      this.cakeDropdownOpen = false;
    }
  }

  onFillingKeyDown(event: KeyboardEvent): void {
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      if (!this.fillingDropdownOpen) this.onFillingFocus();
      this.fillingActiveIndex = Math.min(this.fillingActiveIndex + 1, this.fillingFiltered.length - 1);
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.fillingActiveIndex = Math.max(this.fillingActiveIndex - 1, -1);
    } else if (event.key === 'Enter') {
      if (this.fillingDropdownOpen && this.fillingActiveIndex >= 0 && this.fillingFiltered[this.fillingActiveIndex]) {
        event.preventDefault();
        this.selectFilling(this.fillingFiltered[this.fillingActiveIndex]);
      }
    } else if (event.key === 'Escape') {
      this.fillingDropdownOpen = false;
    }
  }

  loadContacts(): void {
    this.apiService.getContacts().subscribe({
      next: (data: ContactSettings) => {
        this.contacts = data;
        if (data.phone) {
          this.phone = data.phone;
        }
        if (data.email) {
          this.email = data.email;
        }
        if (data.address) {
          this.address = data.address;
        }
      },
      error: () => { }
    });
  }

  getPhoneLink(phone: string): string {
    if (!phone) return '';
    const cleaned = phone.replace(/[^0-9+]/g, '');
    return `tel:${cleaned}`;
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

  async onSubmit(): Promise<void> {
    this.attempted = true;

    if (!this.isValid()) {
      this.errorMessage = 'Пожалуйста, исправьте ошибки в форме';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    try {
      const recaptchaToken = await this.recaptchaService.getToken('order');

      const order: OrderRequest = {
        ...this.orderData,
        cakeId: !this.customOrder ? this.selectedCakeId : undefined,
        fillingId: this.selectedFillingId,
        recaptchaToken
      };

      this.apiService.createOrder(order).subscribe({
        next: () => {
          this.submitted = true;
          this.successMessage = 'Ваш заказ успешно отправлен! Мы свяжемся с вами в ближайшее время.';
          this.isLoading = false;
          this.clearFormState();
          this.resetForm();
        },
        error: (error) => {
          this.errorMessage = 'Произошла ошибка при отправке заказа. Пожалуйста, попробуйте позже.';
          this.isLoading = false;
          console.error('Order error:', error);
        }
      });
    } catch {
      this.errorMessage = 'Не удалось проверить капчу. Попробуйте ещё раз.';
      this.isLoading = false;
    }
  }

  resetForm(): void {
    setTimeout(() => {
      this.clearFormState();
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
      this.selectedCake = null;
      this.selectedFilling = null;
      this.customOrder = true;
      this.cakeSearch = '';
      this.fillingSearch = '';
      this.cakeFiltered = [];
      this.fillingFiltered = [];
      this.cakeDropdownOpen = false;
      this.fillingDropdownOpen = false;
      this.cakeActiveIndex = -1;
      this.fillingActiveIndex = -1;
      this.datePickerOpen = false;
      this.attempted = false;
      this.touched = {};
    }, 5000);
  }
}
