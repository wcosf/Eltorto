import { Component, OnInit } from '@angular/core';
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
import { AdminNotificationService } from '../../../shared/services/admin-notification.service';
import { ApiService, Category } from '../../../../services/api.service';
import { TableConfig, TableAction } from '../../../shared/models/table-config.model';
import { FormConfig, FormField } from '../../../shared/models/form-config.model';

@Component({
  selector: 'app-category-list',
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
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.scss']
})
export class CategoryListComponent implements OnInit {
  categories: Category[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  loading = false;
  searchTerm: string = '';

  tableConfig!: TableConfig<Category>;

  constructor(
    private apiService: ApiService,
    private dialog: MatDialog,
    private notification: AdminNotificationService
  ) {}

  ngOnInit(): void {
    this.initTableConfig();
    this.loadCategories();
  }

  private initTableConfig(): void {
    const actions: TableAction<Category>[] = [
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
        action: (row) => this.deleteCategory(row),
        condition: (row) => row.id > 0
      }
    ];

    this.tableConfig = {
      columns: [
        { key: 'id', label: 'ID', sortable: true, sticky: true },
        { key: 'name', label: 'Название', sortable: true},
        { key: 'slug', label: 'Slug', sortable: true },
        { key: 'sortOrder', label: 'Порядок', sortable: true },
        {
          key: 'description',
          label: 'Описание',
          sortable: false,
          format: (value) => value?.length > 50 ? value.substring(0, 50) + '...' : value || ''
        }
      ],
      actions,
      pageSizeOptions: [5, 10, 25, 50],
      defaultPageSize: 10,
      enableSort: true
    };
  }

  loadCategories(): void {
    this.loading = true;
    this.apiService.getCategories()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (data) => {
          this.categories = data.sort((a, b) => b.id - a.id);
          this.totalCount = this.categories.length;
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
        this.createCategory(result);
      }
    });
  }

  openEditDialog(category: Category): void {
    const formConfig = this.getFormConfig(category);
    this.dialog.open(FormModalComponent, {
      width: '600px',
      data: {
        config: formConfig,
        initialValue: category
      }
    }).afterClosed().subscribe((result) => {
      if (result) {
        this.updateCategory(category.id, result);
      }
    });
  }

  private getFormConfig(existing?: Category): FormConfig {
    const isEdit = !!existing;
    const fields: FormField[] = [
      {
        key: 'name',
        label: 'Название',
        type: 'text',
        required: true,
        placeholder: 'Введите название категории'
      },
      {
        key: 'slug',
        label: 'Slug (адресная строка)',
        type: 'text',
        required: true,
        placeholder: 'naprimer-torti',
        hint: 'Используется в URL. Только латиница, цифры и дефис. Рекомендуется задать один раз и не менять в дальнейшем.',
        disabled: isEdit
      },
      {
        key: 'description',
        label: 'Описание',
        type: 'textarea',
        required: false,
        rows: 3,
        placeholder: 'Краткое описание категории'
      },
      {
        key: 'sortOrder',
        label: 'Порядок сортировки',
        type: 'number',
        required: false,
        defaultValue: 0,
        placeholder: 'Чем меньше число, тем выше в списке',
        hint: 'Чем меньше число, тем выше категория будет отображаться в списке.'
      }
    ];

    return {
      title: isEdit ? 'Редактировать категорию' : 'Создать категорию',
      fields,
      submitLabel: isEdit ? 'Обновить' : 'Создать',
      cancelLabel: 'Отмена'
    };
  }

  private createCategory(data: Partial<Category>): void {
    const payload: Partial<Category> = {
      name: data.name,
      slug: data.slug,
      description: data.description || '',
      sortOrder: data.sortOrder !== undefined ? Number(data.sortOrder) : 0
    };

    this.loading = true;
    this.apiService.createCategory(payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Категория создана');
          this.loadCategories();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Create error:', err);
        }
      });
  }

  private updateCategory(id: number, data: Partial<Category>): void {
    const original = this.categories.find(c => c.id === id);
    if (!original) {
      this.notification.error('Категория не найдена');
      return;
    }

    const payload = {
      id: id,
      name: data.name ?? original.name,
      slug: data.slug ?? original.slug,
      description: data.description !== undefined ? data.description : original.description,
      sortOrder: data.sortOrder !== undefined ? Number(data.sortOrder) : original.sortOrder
    };

    this.loading = true;
    this.apiService.updateCategory(id, payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Категория обновлена');
          this.loadCategories();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Update error:', err);
        }
      });
  }

  deleteCategory(category: Category): void {
    this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Удаление категории',
        message: `Вы уверены, что хотите удалить категорию "${category.name}"? Это действие необратимо.`,
        confirmLabel: 'Удалить',
        cancelLabel: 'Отмена',
        confirmColor: 'warn'
      }
    }).afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.performDelete(category.id);
      }
    });
  }

  private performDelete(id: number): void {
    this.loading = true;
    this.apiService.deleteCategory(id)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: () => {
          this.notification.success('Категория удалена');
          this.loadCategories();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Delete error:', err);
        }
      });
  }

  onTableAction(event: { action: TableAction<Category>; row: Category }) {
    event.action.action(event.row);
  }
}
