import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
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
import { AdminNotificationService } from '../../../shared/services/admin-notification.service';
import { ApiService, Filling } from '../../../../services/api.service';
import { TableConfig, TableAction } from '../../../shared/models/table-config.model';
import { FormConfig, FormField } from '../../../shared/models/form-config.model';

@Component({
  selector: 'app-filling-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DataTableComponent,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './filling-list.component.html',
  styleUrls: ['./filling-list.component.scss']
})
export class FillingListComponent implements OnInit {
  @ViewChild('imageTemplate', { static: true }) imageTemplate!: TemplateRef<any>;

  fillings: Filling[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  loading = false;
  searchTerm = '';
  tableConfig!: TableConfig<Filling>;
  columnTemplates: { [key: string]: TemplateRef<any> } = {};

  constructor(
    private apiService: ApiService,
    private dialog: MatDialog,
    private notification: AdminNotificationService
  ) {}

  ngOnInit(): void {
    this.initTableConfig();
    this.columnTemplates = { imageUrl: this.imageTemplate };
    this.loadFillings();
  }

  private initTableConfig(): void {
    const actions: TableAction<Filling>[] = [
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
        action: (row) => this.deleteFilling(row),
        condition: (row) => row.id > 0
      }
    ];

    this.tableConfig = {
      columns: [
        { key: 'id', label: 'ID', sortable: true, sticky: true },
        { key: 'imageUrl', label: 'Фото', sortable: false },
        { key: 'name', label: 'Название', sortable: true },
        {
          key: 'description',
          label: 'Описание',
          sortable: false,
          format: (value) => value?.length > 50 ? value.substring(0, 50) + '...' : value || ''
        },
        {
          key: 'hasCrossSection',
          label: 'Хит',
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

  loadFillings(): void {
    this.loading = true;
    this.apiService.getAvailableFillings()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (data) => {
          this.fillings = data;
          this.totalCount = data.length;
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Load error:', err);
        }
      });
  }

  onSearch(): void {
  }

  clearSearch(): void {
    this.searchTerm = '';
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
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
        this.createFilling(result);
      }
    });
  }

  openEditDialog(filling: Filling): void {
    const formConfig = this.getFormConfig(filling);
    this.dialog.open(FormModalComponent, {
      width: '600px',
      data: {
        config: formConfig,
        initialValue: filling
      }
    }).afterClosed().subscribe((result) => {
      if (result) {
        this.updateFilling(filling.id, result);
      }
    });
  }

  private getFormConfig(existing?: Filling): FormConfig {
    const isEdit = !!existing;
    const fields: FormField[] = [
      {
        key: 'name',
        label: 'Название',
        type: 'text',
        required: true,
        placeholder: 'Введите название начинки'
      },
      {
        key: 'description',
        label: 'Описание',
        type: 'textarea',
        required: false,
        rows: 3,
        placeholder: 'Краткое описание начинки'
      },
      {
        key: 'imageUrl',
        label: 'Имя файла изображения',
        type: 'text',
        required: false,
        placeholder: 'например, my_filling.jpg',
        hint: 'Файл должен лежать в папке /images/fillings/'
      },
      {
        key: 'imageFile',
        label: 'Загрузить новое изображение (пока не реализовано)',
        type: 'file',
        required: false,
        fileAccept: 'image/*',
        hint: 'Выберите файл для загрузки (загрузка на сервер пока не реализована)'
      },
      {
        key: 'hasCrossSection',
        label: 'Показывать как "в разрезе" (хит)',
        type: 'checkbox',
        required: false,
        defaultValue: false
      }
    ];

    return {
      title: isEdit ? 'Редактировать начинку' : 'Создать начинку',
      fields,
      submitLabel: isEdit ? 'Обновить' : 'Создать',
      cancelLabel: 'Отмена'
    };
  }

  private createFilling(data: any): void {
    const payload: Partial<Filling> = {
      name: data.name,
      description: data.description || '',
      hasCrossSection: data.hasCrossSection || false,
      imageUrl: data.imageUrl || ''
    };

    if (data.imageFile) {
      this.notification.warning('Загрузка файлов пока не реализована, будет сохранено только имя файла');
    }

    this.loading = true;
    this.apiService.createFilling(payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Начинка создана');
          this.loadFillings();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Create error:', err);
        }
      });
  }

  private updateFilling(id: number, data: any): void {
    const original = this.fillings.find(f => f.id === id);
    if (!original) {
      this.notification.error('Начинка не найдена');
      return;
    }

    const payload: Partial<Filling> = {
      name: data.name ?? original.name,
      description: data.description !== undefined ? data.description : original.description,
      hasCrossSection: data.hasCrossSection !== undefined ? data.hasCrossSection : original.hasCrossSection,
      imageUrl: data.imageUrl ?? original.imageUrl
    };

    if (data.imageFile) {
      this.notification.warning('Загрузка файлов пока не реализована, имя файла не изменится');
    }

    this.loading = true;
    this.apiService.updateFilling(id, payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Начинка обновлена');
          this.loadFillings();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Update error:', err);
        }
      });
  }

  deleteFilling(filling: Filling): void {
    this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Удаление начинки',
        message: `Вы уверены, что хотите удалить начинку "${filling.name}"? Это действие необратимо.`,
        confirmLabel: 'Удалить',
        cancelLabel: 'Отмена',
        confirmColor: 'warn'
      }
    }).afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.performDelete(filling.id);
      }
    });
  }

  private performDelete(id: number): void {
    this.loading = true;
    this.apiService.deleteFilling(id)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Начинка удалена');
          this.loadFillings();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Delete error:', err);
        }
      });
  }

  onTableAction(event: { action: TableAction<Filling>; row: Filling }): void {
    event.action.action(event.row);
  }

  openImagePreview(imageUrl: string, name: string): void {
    if (!imageUrl) return;
    const fullUrl = `/images/fillings/${imageUrl}`;
    this.dialog.open(ImagePreviewDialogComponent, {
      data: { imageUrl: fullUrl, alt: name },
      panelClass: 'image-preview-dialog'
    });
  }

  handleImageError(event: Event, row: Filling): void {
    row.imageUrl = '';
  }
}
