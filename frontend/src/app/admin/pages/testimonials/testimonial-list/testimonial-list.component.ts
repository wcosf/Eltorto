import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { finalize, map } from 'rxjs/operators';

import { DataTableComponent } from '../../../shared/components/data-table/data-table.component';
import { FormModalComponent } from '../../../shared/components/form-modal/form-modal.component';
import { ConfirmationDialogComponent } from '../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { RecentActionsComponent } from '../../../shared/components/recent-actions/recent-actions.component';
import { AdminNotificationService } from '../../../shared/services/admin-notification.service';
import { RecentActionsService, RecentAction } from '../../../../core/recent-actions.service';
import { ApiService, Testimonial, PaginatedResponse } from '../../../../services/api.service';
import { TableConfig, TableAction } from '../../../shared/models/table-config.model';
import { FormConfig, FormField } from '../../../shared/models/form-config.model';

@Component({
  selector: 'app-testimonial-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DataTableComponent,
    RecentActionsComponent,
    StatusBadgeComponent,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './testimonial-list.component.html',
  styleUrls: ['./testimonial-list.component.scss']
})
export class TestimonialListComponent implements OnInit {
  @ViewChild('statusTemplate', { static: true }) statusTemplate!: TemplateRef<any>;
  @ViewChild('responseTemplate', { static: true }) responseTemplate!: TemplateRef<any>;

  allTestimonials: Testimonial[] = [];
  filteredTestimonials: Testimonial[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  loading = false;

  searchTerm = '';
  statusFilter: 'all' | 'approved' | 'pending' = 'all';

  tableConfig!: TableConfig<Testimonial>;
  columnTemplates: { [key: string]: TemplateRef<any> } = {};

  constructor(
    private apiService: ApiService,
    private dialog: MatDialog,
    private notification: AdminNotificationService,
    private recentActions: RecentActionsService
  ) { }

  ngOnInit(): void {
    this.initTableConfig();
    this.columnTemplates = {
      status: this.statusTemplate,
      response: this.responseTemplate,
    };
    this.loadTestimonials();
  }

  private initTableConfig(): void {
    const actions: TableAction<Testimonial>[] = [
      {
        label: 'Одобрить',
        icon: 'check_circle',
        color: 'primary',
        action: (row) => this.toggleApproval(row),
        condition: (row) => !row.isApproved,
        group: 'approval',
        cssClass: 'btn-approve'
      },
      {
        label: 'Отклонить',
        icon: 'cancel',
        color: 'warn',
        action: (row) => this.toggleApproval(row),
        condition: (row) => row.isApproved,
        group: 'approval',
        cssClass: 'btn-reject'
      },
      {
        label: 'Редактировать',
        icon: 'edit',
        color: 'primary',
        action: (row) => this.openEditDialog(row),
        group: 'edit'
      },
      {
        label: 'Удалить',
        icon: 'delete',
        color: 'warn',
        action: (row) => this.deleteTestimonial(row),
        condition: (row) => row.id > 0,
        group: 'edit'
      }
    ];

    this.tableConfig = {
      columns: [
        { key: 'id', label: 'ID', sortable: true, sticky: true },
        { key: 'author', label: 'Автор', sortable: true },
        {
          key: 'text',
          label: 'Текст',
          sortable: false,
          format: (value) => value?.length > 100 ? value.substring(0, 100) + '...' : value || ''
        },
        {
          key: 'date',
          label: 'Дата',
          sortable: true,
          format: (value) => value ? new Date(value).toLocaleDateString('ru-RU') : ''
        },
        { key: 'status', label: 'Статус', sortable: false },
        { key: 'response', label: 'Ответ кондитера', sortable: false }
      ],
      actions,
      pageSizeOptions: [5, 10, 25, 50],
      defaultPageSize: 10,
      enableSort: true
    };
  }

  loadTestimonials(): void {
    this.loading = true;
    this.apiService.getTestimonialsAdmin(1, 1000)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (response: PaginatedResponse<Testimonial>) => {
          this.allTestimonials = response.items;
          this.applyFilters();
        },
        error: (err) => {
          const msg = this.extractErrorMessage(err);
          this.notification.error(msg);
          console.error('Load error:', err);
        }
      });
  }

  applyFilters(): void {
    let filtered = this.allTestimonials;

    if (this.statusFilter !== 'all') {
      const isApproved = this.statusFilter === 'approved';
      filtered = filtered.filter(t => t.isApproved === isApproved);
    }

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(t =>
        t.author.toLowerCase().includes(term) ||
        t.text.toLowerCase().includes(term)
      );
    }

    this.filteredTestimonials = filtered;
    this.totalCount = filtered.length;
    this.pageIndex = 0;
  }

  onSearch(): void {
    this.applyFilters();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.applyFilters();
  }

  onStatusFilterChange(): void {
    this.applyFilters();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
  }

  toggleApproval(testimonial: Testimonial): void {
    const newStatus = !testimonial.isApproved;

    this.apiService.approveTestimonial(testimonial.id, newStatus).subscribe({
      next: () => {
        const updated = { ...testimonial, isApproved: newStatus };
        this.allTestimonials = this.allTestimonials.map(t => t.id === testimonial.id ? updated : t);
        this.filteredTestimonials = this.filteredTestimonials.map(t => t.id === testimonial.id ? updated : t);
        this.notification.success(`Отзыв ${newStatus ? 'одобрен' : 'отклонён'}`);
        this.recentActions.addAction({ type: 'update', entityType: 'отзыв', entityId: testimonial.id, entityName: testimonial.author, link: '/admin/testimonials' });
      },
      error: (err) => this.notification.error(this.extractErrorMessage(err))
    });
  }


  openCreateDialog(): void {
    const formConfig = this.getFormConfig();
    this.dialog.open(FormModalComponent, {
      width: '600px',
      data: { config: formConfig }
    }).afterClosed().subscribe((result) => {
      if (result) {
        this.createTestimonial(result);
      }
    });
  }

  openEditDialog(testimonial: Testimonial): void {
    const formConfig = this.getFormConfig(testimonial);
    formConfig.initialValue = { ...testimonial };
    this.dialog.open(FormModalComponent, {
      width: '600px',
      data: { config: formConfig }
    }).afterClosed().subscribe((result) => {
      if (result) {
        this.updateTestimonial(testimonial.id, result);
      }
    });
  }

  private getFormConfig(existing?: Testimonial): FormConfig {
    const isEdit = !!existing;
    const fields: FormField[] = [
      {
        key: 'author',
        label: 'Имя автора',
        type: 'text',
        required: true,
        placeholder: 'Введите имя'
      },
      {
        key: 'text',
        label: 'Текст отзыва',
        type: 'textarea',
        required: true,
        rows: 5,
        placeholder: 'Введите текст отзыва'
      },
      {
        key: 'response',
        label: 'Ответ кондитера',
        type: 'textarea',
        required: false,
        rows: 3,
        placeholder: 'Ваш ответ на отзыв (необязательно)'
      },
      {
        key: 'isApproved',
        label: 'Одобрен',
        type: 'checkbox',
        required: false,
        defaultValue: false
      }
    ];

    return {
      title: isEdit ? 'Редактировать отзыв' : 'Добавить отзыв',
      fields,
      submitLabel: isEdit ? 'Обновить' : 'Создать',
      cancelLabel: 'Отмена'
    };
  }

  private createTestimonial(data: any): void {
    const payload = {
      Author: data.author,
      Text: data.text,
      Response: data.response || '',
      IsApproved: data.isApproved ?? false
    };

    this.apiService.createTestimonial(payload as any).subscribe({
      next: (created) => {
        this.allTestimonials = [created, ...this.allTestimonials];
        this.applyFilters();
        this.notification.success('Отзыв создан');
        this.recentActions.addAction({ type: 'create', entityType: 'отзыв', entityId: created.id, entityName: created.author, link: '/admin/testimonials' });
      },
      error: (err) => this.notification.error(this.extractErrorMessage(err))
    });
  }

  private updateTestimonial(id: number, data: any): void {
    const payload = {
      Id: id,
      Author: data.author,
      Text: data.text,
      Response: data.response || '',
      IsApproved: data.isApproved ?? false
    };

    this.apiService.updateTestimonial(id, payload as any).subscribe({
      next: (updated) => {
        this.allTestimonials = this.allTestimonials.map(t => t.id === id ? updated : t);
        this.filteredTestimonials = this.filteredTestimonials.map(t => t.id === id ? updated : t);
        this.notification.success('Отзыв обновлён');
        this.recentActions.addAction({ type: 'update', entityType: 'отзыв', entityId: updated.id, entityName: updated.author, link: '/admin/testimonials' });
      },
      error: (err) => this.notification.error(this.extractErrorMessage(err))
    });
  }


  deleteTestimonial(testimonial: Testimonial): void {
    this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Удаление отзыва',
        message: `Вы уверены, что хотите удалить отзыв "${testimonial.author}"? Это действие необратимо.`,
        confirmLabel: 'Удалить',
        cancelLabel: 'Отмена',
        confirmColor: 'warn'
      }
    }).afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.performDelete(testimonial.id, testimonial.author);
      }
    });
  }

  private performDelete(id: number, author: string): void {
    this.apiService.deleteTestimonial(id).subscribe({
      next: () => {
        this.allTestimonials = this.allTestimonials.filter(t => t.id !== id);
        this.filteredTestimonials = this.filteredTestimonials.filter(t => t.id !== id);
        this.totalCount = this.filteredTestimonials.length;
        this.notification.success('Отзыв удалён');
        this.recentActions.addAction({
          type: 'delete',
          entityType: 'отзыв',
          entityId: id,
          entityName: author,
          link: null
        });
      },
      error: (err) => {
        const msg = this.extractErrorMessage(err);
        this.notification.error(msg);
        console.error('Delete error:', err);
      }
    });
  }

  onTableAction(event: { action: TableAction<Testimonial>; row: Testimonial }): void {
    event.action.action(event.row);
  }

  onRecentActionClick(action: RecentAction): void {
    if (action.type === 'create' || action.type === 'update') {
      const index = this.filteredTestimonials.findIndex(t => t.id === action.entityId);
      if (index !== -1) {
        const page = Math.floor(index / this.pageSize);
        this.pageIndex = page;
        setTimeout(() => {
          this.highlightRow(action.entityId);
          const tableElement = document.querySelector('.data-table-container');
          if (tableElement) {
            tableElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
          }
        }, 300);
      } else {
        this.notification.warning('Отзыв не найден, возможно, данные не загружены');
      }
    }
  }

  private highlightRow(id: number): void {
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
