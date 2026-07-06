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
    public apiService: ApiService,
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
      console.log('Dialog result:', result);
      if (result) {
        const imageFile = result._file || result.imageFile;
        console.log('ImageFile from dialog:', imageFile);
        this.createFilling({ ...result, imageFile });
      } else {
        console.log('Dialog closed without result');
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
        const imageFile = result._file || result.imageFile;
        this.updateFilling(filling.id, { ...result, imageFile });
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
        disabled: isEdit
      },
      {
        key: 'imageFile',
        label: 'Загрузить новое изображение',
        type: 'file',
        required: false,
        fileAccept: 'image/*',
        hint: 'Выберите файл для загрузки (jpg, png, webp, до 5 МБ)'
      },
      {
        key: 'hasCrossSection',
        label: 'Показывать как "хит"',
        type: 'checkbox',
        required: false,
        defaultValue: false
      },
      {
        key: 'removeImage',
        label: 'Удалить текущее изображение',
        type: 'checkbox',
        required: false,
        defaultValue: false,
        hint: 'Если отмечено, текущее фото будет удалено'
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
    console.log('Create data:', data);
    const payload: Partial<Filling> = {
      name: data.name,
      description: data.description || '',
      hasCrossSection: data.hasCrossSection || false,
      imageUrl: data.imageUrl || ''
    };

    if (data.imageFile && data.imageFile instanceof File) {
      this.loading = true;
      this.apiService.uploadFile(data.imageFile, 'fillings').subscribe({
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

  private sendCreateRequest(payload: Partial<Filling>): void {
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
    console.log('Update data:', data);
    const original = this.fillings.find(f => f.id === id);
    if (!original) {
      this.notification.error('Начинка не найдена');
      return;
    }

    if (data.imageFile && data.imageFile instanceof File) {
      this.loading = true;
      this.apiService.uploadFile(data.imageFile, 'fillings', id).subscribe({
        next: (uploadRes) => {
          const payload: Partial<Filling> = {
            id: id,
            name: data.name ?? original.name,
            description: data.description !== undefined ? data.description : original.description,
            hasCrossSection: data.hasCrossSection !== undefined ? data.hasCrossSection : original.hasCrossSection,
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

    if (data.removeImage === true) {
      this.loading = true;
      this.apiService.deleteFillingImage(id).subscribe({
        next: () => {
          const payload: Partial<Filling> = {
            id: id,
            name: data.name ?? original.name,
            description: data.description !== undefined ? data.description : original.description,
            hasCrossSection: data.hasCrossSection !== undefined ? data.hasCrossSection : original.hasCrossSection,
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

    const payload: Partial<Filling> = {
      id: id,
      name: data.name ?? original.name,
      description: data.description !== undefined ? data.description : original.description,
      hasCrossSection: data.hasCrossSection !== undefined ? data.hasCrossSection : original.hasCrossSection,
      imageUrl: data.imageUrl ?? original.imageUrl
    };
    this.sendUpdateRequest(id, payload);
  }

  private sendUpdateRequest(id: number, payload: Partial<Filling>): void {
    const updatePayload = { ...payload, id };
    this.loading = true;
    this.apiService.updateFilling(id, updatePayload)
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
    const fullUrl = this.apiService.getFillingImageUrl(imageUrl);
    this.dialog.open(ImagePreviewDialogComponent, {
      data: { imageUrl: fullUrl, alt: name },
      panelClass: 'image-preview-dialog'
    });
  }

  handleImageError(event: Event, row: Filling): void {
    row.imageUrl = '';
  }
}
