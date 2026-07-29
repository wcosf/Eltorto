import { Component, OnInit, OnDestroy, ViewChild, TemplateRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule } from '@angular/material/dialog';
import { finalize, map } from 'rxjs/operators';
import { Observable } from 'rxjs';

import { DataTableComponent } from '../../../shared/components/data-table/data-table.component';
import { ConfirmationDialogComponent } from '../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { StatusBadgeComponent, StatusType } from '../../../shared/components/status-badge/status-badge.component';
import { FormModalComponent } from '../../../shared/components/form-modal/form-modal.component';
import { OrderStatusDialogComponent } from '../order-status-dialog/order-status-dialog.component';
import { OrderDetailDialogComponent } from '../order-detail-dialog/order-detail-dialog.component';
import { ImagePreviewDialogComponent } from '../../../shared/components/image-preview-dialog/image-preview-dialog.component';
import { AdminNotificationService } from '../../../shared/services/admin-notification.service';
import { AdminStateService } from '../../../shared/services/admin-state.service';
import { ApiService, Cake, Filling, OrderDto, OrderRequest, OrderStatus } from '../../../../services/api.service';
import { TableConfig, TableAction } from '../../../shared/models/table-config.model';
import { FormConfig, FormField, FormFieldOption } from '../../../shared/models/form-config.model';
import { futureDateValidator } from '../../../shared/validators/date.validators';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    DataTableComponent,
    StatusBadgeComponent,
    ConfirmationDialogComponent,
    FormModalComponent,
  ],
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss']
})
export class OrderListComponent implements OnInit, OnDestroy {
  @ViewChild('statusTemplate', { static: true }) statusTemplate!: TemplateRef<any>;
  @ViewChild('cakeTemplate', { static: true }) cakeTemplate!: TemplateRef<any>;

  orders: OrderDto[] = [];
  totalCount = 0;
  pageSize = 25;
  pageIndex = 0;
  loading = false;
  searchTerm: string = '';
  cakes: Cake[] = [];
  fillings: Filling[] = [];

  selectedStatusSignal = signal<string | null>(null);

  statusFilterOptions = [
    { value: null, label: 'Все' },
    { value: 'New', label: 'Новый' },
    { value: 'Processing', label: 'В обработке' },
    { value: 'Completed', label: 'Завершён' },
    { value: 'Cancelled', label: 'Отменён' },
  ];

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

  tableConfig!: TableConfig<OrderDto>;
  columnTemplates: { [key: string]: TemplateRef<any> } = {};

  constructor(
    public apiService: ApiService,
    private dialog: MatDialog,
    private notification: AdminNotificationService,
    private stateService: AdminStateService
  ) { }

  ngOnInit(): void {
    this.restoreTableState();
    this.initTableConfig();
    this.columnTemplates = { status: this.statusTemplate, cakeName: this.cakeTemplate };
    this.loadOrders();
    this.loadCakes();
    this.loadFillings();
  }

  ngOnDestroy(): void {
    this.stateService.saveTableState('orders', {
      pageIndex: this.pageIndex,
      pageSize: this.pageSize
    });
  }

  private restoreTableState(): void {
    const saved = this.stateService.getTableState('orders');
    if (saved) {
      this.pageIndex = saved.pageIndex;
      this.pageSize = saved.pageSize;
    }
  }

  private initTableConfig(): void {
    const actions: TableAction<OrderDto>[] = [
      {
        label: 'Просмотр',
        icon: 'visibility',
        color: 'primary',
        action: (row) => this.viewOrder(row),
      },
      {
        label: 'Изменить статус',
        icon: 'sync_alt',
        color: 'primary',
        action: (row) => this.openStatusDialog(row),
      },
      {
        label: 'Редактировать',
        icon: 'edit',
        color: 'primary',
        action: (row) => this.openEditDialog(row),
      },
      {
        label: 'Удалить',
        icon: 'delete',
        color: 'warn',
        action: (row) => this.deleteOrder(row),
      },
    ];

    this.tableConfig = {
      columns: [
        { key: 'id', label: 'ID', sortable: true },
        { key: 'createdAt', label: 'Дата', sortable: true, format: (value) => this.formatDate(value) },
        { key: 'customerName', label: 'Имя', sortable: true },
        { key: 'customerPhone', label: 'Телефон', sortable: false },
        {
          key: 'cakeName',
          label: 'Торт/дизайн',
          sortable: false,
          format: (value, row) => value || 'Свой дизайн',
          cssClass: 'cake-column',
        },
        {
          key: 'status',
          label: 'Статус',
          sortable: false,
        },
      ],
      actions,
      pageSizeOptions: [25],
      defaultPageSize: 25,
      enableSort: true,
    };
  }

  openCakePreview(order: OrderDto): void {
    const url = order.cakeImageUrl || this.getCakeImageFromList(order.cakeId);
    if (!url) return;
    const fullUrl = this.apiService.getCakeImageUrl(url);
    this.dialog.open(ImagePreviewDialogComponent, {
      data: { imageUrl: fullUrl, alt: order.cakeName || 'Фото торта' },
      panelClass: 'image-preview-dialog'
    });
  }

  getCakeImageUrl(url: string | undefined): string {
    return url ? this.apiService.getCakeImageUrl(url) : '';
  }

  getCakeImageFromList(cakeId: number | undefined): string {
    if (!cakeId) return '';
    const cake = this.cakes.find(c => c.id === cakeId);
    return cake?.imageUrl ? this.apiService.getCakeImageUrl(cake.imageUrl) : '';
  }

  private loadCakes(): void {
    this.apiService.getCakesPaged(1, 50).subscribe({
      next: (response) => this.cakes = response.items,
      error: () => { },
    });
  }

  private loadFillings(): void {
    this.apiService.getAvailableFillings().subscribe({
      next: (fillings) => this.fillings = fillings,
      error: () => { },
    });
  }

  searchCakes(query: string): Observable<FormFieldOption[]> {
    return this.apiService.getCakesPaged(1, 100, undefined, query || undefined).pipe(
      map(response => response.items.map(cake => ({
        value: cake,
        label: cake.name,
        thumbUrl: cake.imageUrl ? this.apiService.getCakeImageUrl(cake.imageUrl) : undefined,
      })))
    );
  }

  private formatDate(dateStr: string): string {
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

  loadOrders(): void {
    this.loading = true;
    const status = this.selectedStatusSignal();
    this.apiService.getOrdersPaged(1, 10000, status || undefined)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (response) => {
          this.orders = response.items;
          this.totalCount = response.totalCount;
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
        },
      });
  }

  onStatusFilterChange(status: string | null): void {
    this.selectedStatusSignal.set(status);
    this.pageIndex = 0;
    this.loadOrders();
  }

  onSearch(): void {
    this.pageIndex = 0;
  }

  clearSearch(): void {
    this.searchTerm = '';
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    this.pageIndex = 0;
  }

  private getOrderFormConfig(order?: OrderDto): FormConfig {
    const isEdit = !!order;

    const fillingOptions: FormFieldOption[] = this.fillings.map(filling => ({
      value: filling,
      label: filling.name,
    }));

    const fields: FormField[] = [
      {
        key: 'customerName',
        label: 'Имя',
        type: 'text',
        required: true,
        placeholder: 'Введите имя',
        validators: [Validators.minLength(2)],
        validationMessages: {
          required: 'Имя обязательно',
          minlength: 'Минимальная длина имени: 2 симв.'
        }
      },
      {
        key: 'customerPhone',
        label: 'Телефон',
        type: 'text',
        required: true,
        placeholder: '+7 (999) 999-99-99',
        validators: [Validators.pattern(/^(\+7|8)\s?\(?\d{3}\)?\s?\d{3}[\s-]?\d{2}[\s-]?\d{2}$/)],
        validationMessages: {
          required: 'Телефон обязателен',
          pattern: 'Некорректный формат телефона'
        }
      },
      {
        key: 'customerEmail',
        label: 'Email',
        type: 'email',
        required: true,
        placeholder: 'Введите email',
        validators: [Validators.email],
        validationMessages: {
          required: 'Email обязателен',
          email: 'Некорректный формат email'
        }
      },
      {
        key: 'cakeId',
        label: 'Торт',
        type: 'autocomplete',
        asyncOptionsFn: (query: string) => this.searchCakes(query),
        displayFn: (cake: Cake) => cake?.name || '',
        showImagePreview: true,
      },
      {
        key: 'fillingId',
        label: 'Начинка',
        type: 'autocomplete',
        options: fillingOptions,
        displayFn: (filling: Filling) => filling?.name || '',
      },
      {
        key: 'weight',
        label: 'Вес (кг)',
        type: 'number',
        placeholder: 'Введите вес',
      },
      {
        key: 'customCakeDescription',
        label: 'Описание своего дизайна',
        type: 'textarea',
        placeholder: 'Опишите свой дизайн',
        rows: 4,
      },
      {
        key: 'deliveryDate',
        label: 'Дата доставки',
        type: 'date',
        validators: [futureDateValidator()],
        validationMessages: {
          pastDate: 'Дата доставки не может быть в прошлом'
        }
      },
      {
        key: 'deliveryAddress',
        label: 'Адрес доставки',
        type: 'text',
        required: true,
        placeholder: 'Введите адрес',
      },
      {
        key: 'comment',
        label: 'Комментарий',
        type: 'textarea',
        placeholder: 'Дополнительный комментарий',
        rows: 4,
      },
    ];

    return {
      title: isEdit ? 'Редактировать заказ' : 'Создать заказ',
      fields,
      submitLabel: isEdit ? 'Сохранить' : 'Создать',
      cancelLabel: 'Отмена',
    };
  }

  private getOrderInitialValue(order: OrderDto): any {
    const selectedCake: Cake | null = order.cakeId ? {
      id: order.cakeId,
      name: order.cakeName || '',
      imageUrl: order.cakeImageUrl || '',
      thumbnailUrl: '',
      categorySlug: '',
      isFeatured: false,
    } : null;
    const selectedFilling = this.fillings.find(f => f.id === order.fillingId);

    return {
      customerName: order.customerName,
      customerPhone: order.customerPhone,
      customerEmail: order.customerEmail,
      cakeId: selectedCake || null,
      fillingId: selectedFilling || null,
      weight: order.weight,
      customCakeDescription: order.customCakeDescription || '',
      deliveryDate: order.deliveryDate ? new Date(order.deliveryDate) : null,
      deliveryAddress: order.deliveryAddress || '',
      comment: order.comment || '',
    };
  }

  private buildOrderRequest(data: any): OrderRequest {
    return {
      customerName: data.customerName,
      customerPhone: data.customerPhone,
      customerEmail: data.customerEmail || undefined,
      cakeId: typeof data.cakeId === 'object' && data.cakeId ? data.cakeId.id : undefined,
      fillingId: typeof data.fillingId === 'object' && data.fillingId ? data.fillingId.id : undefined,
      weight: typeof data.weight === 'number' ? data.weight : undefined,
      customCakeDescription: data.customCakeDescription || undefined,
      deliveryDate: data.deliveryDate instanceof Date ? data.deliveryDate.toISOString() : (data.deliveryDate || undefined),
      deliveryAddress: data.deliveryAddress || undefined,
      comment: data.comment || undefined,
    };
  }

  openCreateDialog(): void {
    const config = this.getOrderFormConfig();
    this.dialog.open(FormModalComponent, {
      width: '600px',
      data: { config },
    }).afterClosed().subscribe((result) => {
      if (result) {
        this.createOrder(result);
      }
    });
  }

  openEditDialog(order: OrderDto): void {
    const config = this.getOrderFormConfig(order);
    const initialValue = this.getOrderInitialValue(order);
    this.dialog.open(FormModalComponent, {
      width: '600px',
      data: { config, initialValue },
    }).afterClosed().subscribe((result) => {
      if (result) {
        this.updateOrder(order.id, result);
      }
    });
  }

  private createOrder(data: any): void {
    const payload = this.buildOrderRequest(data);
    this.loading = true;
    this.apiService.createOrder(payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Заказ создан');
          this.loadOrders();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
        },
      });
  }

  private updateOrder(id: number, data: any): void {
    const payload = this.buildOrderRequest(data);
    this.loading = true;
    this.apiService.updateOrder(id, payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Заказ обновлён');
          this.loadOrders();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
        },
      });
  }

  deleteOrder(order: OrderDto): void {
    this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Удалить заказ',
        message: `Вы уверены? Заказ #${order.id} будет удалён`,
        confirmLabel: 'Удалить',
        confirmColor: 'warn',
      },
    }).afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.loading = true;
        this.apiService.deleteOrder(order.id)
          .pipe(finalize(() => this.loading = false))
          .subscribe({
            next: () => {
              this.notification.success('Заказ удалён');
              this.loadOrders();
            },
            error: (err) => {
              const msg = this.extractErrorMessage(err);
              this.notification.error(msg);
            },
          });
      }
    });
  }

  openStatusDialog(order: OrderDto): void {
    this.dialog.open(OrderStatusDialogComponent, {
      width: '400px',
      data: { currentStatus: order.status },
    }).afterClosed().subscribe((newStatus: OrderStatus | undefined) => {
      if (newStatus && newStatus !== order.status) {
        this.updateOrderStatus(order.id, newStatus);
      }
    });
  }

  private updateOrderStatus(id: number, status: OrderStatus): void {
    this.loading = true;
    this.apiService.updateOrderStatus(id, status)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Статус заказа обновлён');
          this.loadOrders();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
        },
      });
  }

  viewOrder(order: OrderDto): void {
    this.dialog.open(OrderDetailDialogComponent, {
      width: '600px',
      data: { order },
    });
  }

  onTableAction(event: { action: TableAction<OrderDto>; row: OrderDto }): void {
    event.action.action(event.row);
  }

  private extractErrorMessage(err: any): string {
    if (err.error) {
      const body = err.error;
      if (body.errors) {
        const messages = Object.values(body.errors).flat() as string[];
        if (messages.length) {
          return messages.join('; ');
        }
      }
      if (body.error) return body.error;
      if (body.title) return body.title;
      if (body.message) return body.message;
      if (typeof body === 'string') return body;
    }
    if (err.message) return err.message;
    return 'Произошла ошибка';
  }
}
