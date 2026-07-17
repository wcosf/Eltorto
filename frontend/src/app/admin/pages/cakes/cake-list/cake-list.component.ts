import { Component, OnInit, ViewChild, TemplateRef, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { finalize } from 'rxjs/operators';

import { DataTableComponent } from '../../../shared/components/data-table/data-table.component';
import { FormModalComponent } from '../../../shared/components/form-modal/form-modal.component';
import { ConfirmationDialogComponent } from '../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { ImagePreviewDialogComponent } from '../../../shared/components/image-preview-dialog/image-preview-dialog.component';
import { RecentActionsComponent } from '../../../shared/components/recent-actions/recent-actions.component';
import { AdminNotificationService } from '../../../shared/services/admin-notification.service';
import { RecentActionsService } from '../../../../core/recent-actions.service';
import { RecentAction } from '../../../../core/recent-actions.service';
import { ApiService, Cake, Category, Filling } from '../../../../services/api.service';
import { TableConfig, TableAction } from '../../../shared/models/table-config.model';
import { FormConfig, FormField } from '../../../shared/models/form-config.model';

@Component({
  selector: 'app-cake-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DataTableComponent,
    RecentActionsComponent,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './cake-list.component.html',
  styleUrls: ['./cake-list.component.scss']
})
export class CakeListComponent implements OnInit {
  @ViewChild('imageTemplate', { static: true }) imageTemplate!: TemplateRef<any>;

  cakes: Cake[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  loading = false;
  searchTerm = '';

  categories: Category[] = [];
  fillings: Filling[] = [];

  tableConfig!: TableConfig<Cake>;
  columnTemplates: { [key: string]: TemplateRef<any> } = {};

  constructor(
    public apiService: ApiService,
    private dialog: MatDialog,
    private notification: AdminNotificationService,
    private recentActions: RecentActionsService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.initTableConfig();
    this.columnTemplates = { imageUrl: this.imageTemplate };
    this.loadReferences();
    this.loadCakes();
  }

  private initTableConfig(): void {
    const actions: TableAction<Cake>[] = [
      {
        label: 'Редактировать',
        icon: 'edit',
        color: 'primary',
        action: (row) => this.openEditDialog(row)
      },
      {
        label: 'Удалить',
        icon: 'delete',
        color: 'warn',
        action: (row) => this.deleteCake(row),
        condition: (row) => row.id > 0
      }
    ];

    this.tableConfig = {
      columns: [
        { key: 'id', label: 'ID', sortable: true, sticky: true },
        { key: 'imageUrl', label: 'Фото', sortable: false },
        { key: 'name', label: 'Название', sortable: true },
        {
          key: 'categorySlug',
          label: 'Категория',
          sortable: false,
          format: (value, row) => this.getCategoryName(row.categorySlug)
        },
        {
          key: 'fillingId',
          label: 'Начинка',
          sortable: false,
          format: (value, row) => this.getFillingName(row.fillingId)
        },
        {
          key: 'isFeatured',
          label: 'Рекомендуемый',
          sortable: true,
          format: (value) => value ? '⭐ Да' : 'Нет'
        }
      ],
      actions,
      pageSizeOptions: [5, 10, 25, 50],
      defaultPageSize: 10,
      enableSort: true
    };
  }

  private loadReferences(): void {
    this.apiService.getCategories().subscribe({
      next: (categories) => this.categories = categories,
      error: () => this.notification.warning('Не удалось загрузить категории')
    });

    this.apiService.getAvailableFillings().subscribe({
      next: (fillings) => this.fillings = fillings,
      error: () => this.notification.warning('Не удалось загрузить начинки')
    });
  }

  private getCategoryName(slug: string): string {
    const category = this.categories.find(c => c.slug === slug);
    return category ? category.name : slug || '—';
  }

  private getFillingName(id?: number): string {
    if (!id) return '—';
    const filling = this.fillings.find(f => f.id === id);
    return filling ? filling.name : '—';
  }

  loadCakes(): void {
    this.loading = true;
    this.apiService.getCakesPaged(1, 10000)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (response) => {
          this.cakes = response.items;
          this.totalCount = response.totalCount;
          console.log(`Загружено ${this.cakes.length} тортов`);
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Load error:', err);
        }
      });
  }

  onSearch(): void {
    this.pageIndex = 0;
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.onSearch();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    this.pageIndex = 0;
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

  openCreateDialog(): void {
    const formConfig = this.getFormConfig();
    this.dialog.open(FormModalComponent, {
      width: '600px',
      data: { config: formConfig }
    }).afterClosed().subscribe((result) => {
      if (result) {
        const imageFile = result._file;
        this.createCake({ ...result, imageFile });
      }
    });
  }

  openEditDialog(cake: Cake): void {
    const formConfig = this.getFormConfig(cake);
    this.dialog.open(FormModalComponent, {
      width: '600px',
      data: {
        config: formConfig,
        initialValue: cake
      }
    }).afterClosed().subscribe((result) => {
      if (result) {
        const imageFile = result._file;
        this.updateCake(cake.id, { ...result, imageFile });
      }
    });
  }

  private getFormConfig(existing?: Cake): FormConfig {
    const isEdit = !!existing;

    const categoryOptions = this.categories.map(c => ({
      value: c.slug,
      label: c.name
    }));

    const fillingOptions = this.fillings.map(f => ({
      value: f.id,
      label: f.name
    }));

    const fields: FormField[] = [
      {
        key: 'name',
        label: 'Название',
        type: 'text',
        required: true,
        placeholder: 'Введите название торта'
      },
      {
        key: 'categorySlug',
        label: 'Категория',
        type: 'select',
        required: true,
        options: categoryOptions,
        placeholder: 'Выберите категорию'
      },
      {
        key: 'fillingId',
        label: 'Начинка',
        type: 'select',
        required: false,
        options: fillingOptions,
        placeholder: 'Выберите начинку',
        disabled: true,
        hint: 'Нельзя добавить (временно)'
      },
      {
        key: 'isFeatured',
        label: 'Рекомендуемый торт',
        type: 'checkbox',
        required: false,
        defaultValue: false,
        hint: 'На главной странице отображаются первые 6 рекомендованых тортов'
      },
      {
        key: 'description',
        label: 'Описание',
        type: 'textarea',
        required: false,
        rows: 3,
        placeholder: 'Краткое описание торта'
      },
      {
        key: 'imageFile',
        label: 'Загрузить изображение',
        type: 'file',
        required: false,
        fileAccept: 'image/*',
        hint: 'Выберите файл (jpg, png, webp, до 5 МБ)'
      }
    ];

    if (isEdit) {
      fields.push({
        key: 'removeImage',
        label: 'Удалить текущее изображение',
        type: 'checkbox',
        required: false,
        defaultValue: false,
        hint: 'Если отмечено, текущее фото будет удалено'
      });
    }

    return {
      title: isEdit ? 'Редактировать торт' : 'Создать торт',
      fields,
      submitLabel: isEdit ? 'Обновить' : 'Создать',
      cancelLabel: 'Отмена'
    };
  }

  private createCake(data: any): void {
    const payload: Partial<Cake> = {
      name: data.name,
      categorySlug: data.categorySlug,
      fillingId: data.fillingId || undefined,
      isFeatured: data.isFeatured || false,
      description: data.description || '',
      imageUrl: data.imageUrl || ''
    };

    if (data.imageFile && data.imageFile instanceof File) {
      this.loading = true;
      this.apiService.uploadFile(data.imageFile, 'cakes').subscribe({
        next: (uploadRes) => {
          payload.imageUrl = uploadRes.imageUrl;
          this.sendCreateRequest(payload);
        },
        error: (err) => {
          this.loading = false;
          const msg = this.extractErrorMessage(err);
          this.notification.error('Ошибка загрузки изображения: ' + msg);
          console.error('Upload error:', err);
        }
      });
    } else {
      this.sendCreateRequest(payload);
    }
  }

  private sendCreateRequest(payload: Partial<Cake>): void {
    this.loading = true;
    this.apiService.createCake(payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (created) => {
          this.notification.success('Торт создан');
          this.recentActions.addAction({
            type: 'create',
            entityType: 'торт',
            entityId: created.id,
            entityName: created.name,
            link: '/admin/cakes'
          });
          this.loadCakes();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Create error:', err);
        }
      });
  }

  private updateCake(id: number, data: any): void {
    const original = this.cakes.find(c => c.id === id);
    if (!original) {
      this.notification.error('Торт не найден');
      return;
    }

    let imageUrl = data.imageUrl ?? original.imageUrl;

    if (data.removeImage === true) {
      this.loading = true;
      this.apiService.deleteCakeImage(id).subscribe({
        next: () => {
          const payload: Partial<Cake> = {
            name: data.name ?? original.name,
            categorySlug: data.categorySlug ?? original.categorySlug,
            fillingId: data.fillingId !== undefined ? data.fillingId : original.fillingId,
            isFeatured: data.isFeatured !== undefined ? data.isFeatured : original.isFeatured,
            description: data.description !== undefined ? data.description : original.description,
            imageUrl: ''
          };
          this.sendUpdateRequest(id, payload);
        },
        error: (err) => {
          this.loading = false;
          const msg = this.extractErrorMessage(err);
          this.notification.error('Ошибка удаления изображения: ' + msg);
          console.error('Delete image error:', err);
        }
      });
      return;
    }

    if (data.imageFile && data.imageFile instanceof File) {
      this.loading = true;
      this.apiService.uploadFile(data.imageFile, 'cakes', id).subscribe({
        next: (uploadRes) => {
          const payload: Partial<Cake> = {
            name: data.name ?? original.name,
            categorySlug: data.categorySlug ?? original.categorySlug,
            fillingId: data.fillingId !== undefined ? data.fillingId : original.fillingId,
            isFeatured: data.isFeatured !== undefined ? data.isFeatured : original.isFeatured,
            description: data.description !== undefined ? data.description : original.description,
            imageUrl: uploadRes.imageUrl
          };
          this.sendUpdateRequest(id, payload);
        },
        error: (err) => {
          this.loading = false;
          const msg = this.extractErrorMessage(err);
          this.notification.error('Ошибка загрузки изображения: ' + msg);
          console.error('Upload error:', err);
        }
      });
      return;
    }

    const payload: Partial<Cake> = {
      name: data.name ?? original.name,
      categorySlug: data.categorySlug ?? original.categorySlug,
      fillingId: data.fillingId !== undefined ? data.fillingId : original.fillingId,
      isFeatured: data.isFeatured !== undefined ? data.isFeatured : original.isFeatured,
      description: data.description !== undefined ? data.description : original.description,
      imageUrl: imageUrl
    };
    this.sendUpdateRequest(id, payload);
  }

  private sendUpdateRequest(id: number, payload: Partial<Cake>): void {
    const updatePayload = { ...payload, id };
    this.loading = true;
    this.apiService.updateCake(id, updatePayload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (updated) => {
          this.notification.success('Торт обновлён');
          this.recentActions.addAction({
            type: 'update',
            entityType: 'торт',
            entityId: updated.id,
            entityName: updated.name,
            link: '/admin/cakes'
          });
          this.loadCakes();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Update error:', err);
        }
      });
  }

  deleteCake(cake: Cake): void {
    this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Удаление торта',
        message: `Вы уверены, что хотите удалить торт "${cake.name}"? Это действие необратимо.`,
        confirmLabel: 'Удалить',
        cancelLabel: 'Отмена',
        confirmColor: 'warn'
      }
    }).afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.performDelete(cake.id, cake.name);
      }
    });
  }

  private performDelete(id: number, name: string): void {
    this.loading = true;
    this.apiService.deleteCake(id)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Торт удалён');
          this.recentActions.addAction({
            type: 'delete',
            entityType: 'торт',
            entityId: id,
            entityName: name,
            link: null
          });
          this.loadCakes();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Delete error:', err);
        }
      });
  }

  onTableAction(event: { action: TableAction<Cake>; row: Cake }): void {
    event.action.action(event.row);
  }

  openImagePreview(imageUrl: string, name: string): void {
    if (!imageUrl) return;
    const fullUrl = this.apiService.getCakeImageUrl(imageUrl);
    this.dialog.open(ImagePreviewDialogComponent, {
      data: { imageUrl: fullUrl, alt: name },
      panelClass: 'image-preview-dialog'
    });
  }

  onRecentActionClick(action: RecentAction): void {
    if (action.type === 'create' || action.type === 'update') {
      const index = this.cakes.findIndex(c => c.id === action.entityId);
      if (index !== -1) {
        const page = Math.floor(index / this.pageSize);
        this.pageIndex = page;
        setTimeout(() => {
          this.highlightRow(action.entityId);
          const tableElement = document.querySelector('.data-table-container');
          if (tableElement) {
            tableElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
          }
        }, 100);
      } else {
        this.notification.warning('Торт не найден, возможно, данные не загружены');
      }
    }
  }

  goToCake(id: number): void {
    this.searchTerm = '';
    const index = this.cakes.findIndex(c => c.id === id);
    if (index === -1) {
      this.notification.warning('Торт не найден');
      return;
    }
    const pageIndex = Math.floor(index / this.pageSize);
    this.pageIndex = pageIndex;
    setTimeout(() => {
      this.highlightRow(id);
    }, 300);
  }

  highlightRow(id: number): void {
    document.querySelectorAll('tr.highlight-row').forEach(row => {
      row.classList.remove('highlight-row');
    });

    const row = document.querySelector(`tr[data-id="${id}"]`);
    if (row) {
      setTimeout(() => {
        row.classList.add('highlight-row');
      }, 10);
    }
  }
}
