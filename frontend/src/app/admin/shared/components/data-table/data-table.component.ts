import {
  Component, Input, Output, EventEmitter, OnInit, AfterViewInit,
  OnChanges, SimpleChanges, ViewChild, TemplateRef, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTable, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TableConfig, TableAction, TableColumn } from '../../models/table-config.model';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss']
})
export class DataTableComponent<T> implements OnInit, AfterViewInit, OnChanges {
  @Input() data: T[] = [];
  @Input() totalCount = 0;
  @Input() pageSize = 25;
  @Input() pageIndex = 0;
  @Input() config!: TableConfig<T>;
  @Input() loading = false;
  @Input() filterableColumns: string[] = [];
  @Input() columnTemplates: { [key: string]: TemplateRef<any> } = {};
  @Input() defaultSort?: { active: string; direction: 'asc' | 'desc' };
  @Input() serverSide = false;

  @Output() pageChange = new EventEmitter<{ pageIndex: number; pageSize: number }>();
  @Output() sortChange = new EventEmitter<{ active: string; direction: 'asc' | 'desc' }>();
  @Output() actionClick = new EventEmitter<{ action: TableAction<T>; row: T }>();

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatTable) table!: MatTable<T>;

  private _filterValue = '';
  @Input() set filterValue(value: string) {
    const newValue = value || '';
    if (this._filterValue !== newValue) {
      this._filterValue = newValue;
      if (!this.serverSide) {
        this.pageIndex = 0;
        this.updateDisplayData();
      } else {
        this.pageChange.emit({ pageIndex: 0, pageSize: this.pageSize });
      }
    }
  }
  get filterValue(): string {
    return this._filterValue;
  }

  Math = Math;

  constructor(private cdr: ChangeDetectorRef) {}

  displayedColumns: string[] = [];
  dataSource = new MatTableDataSource<T>([]);
  filteredLength = 0;
  _highlightId: number | null = null;

  private allData: T[] = [];
  private filteredData: T[] = [];

  ngOnInit() {
    this.initTable();
    if (!this.serverSide) {
      this.setupFilter();
      this.allData = this.data;
    }
    this.updateDisplayData();
  }

  ngAfterViewInit() {
    if (!this.serverSide) {
      this.dataSource.sort = this.sort;
      if (this.defaultSort && this.sort) {
        this.sort.active = this.defaultSort.active;
        this.sort.direction = this.defaultSort.direction;
        this.sort.sortChange.emit({ active: this.defaultSort.active, direction: this.defaultSort.direction });
      }
    }
    this.updateDisplayData();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['data']) {
      if (this.serverSide) {
        this.dataSource.data = this.data;
        if (this.table) {
          this.table.renderRows();
        }
      } else {
        this.allData = this.data;
        this.updateDisplayData();
      }
    }
    if (changes['totalCount'] && this.serverSide) {
      this.filteredLength = this.totalCount;
    }
    if (changes['pageSize'] || changes['pageIndex']) {
      if (!this.serverSide) {
        this.updateDisplayData();
      }
    }
  }

  private initTable() {
    this.displayedColumns = this.config.columns.map(c => c.key as string);
    if (this.config.actions?.length) {
      this.displayedColumns.push('actions');
    }
    this.dataSource = new MatTableDataSource<T>([]);
  }

  private setupFilter() {
    if (this.filterableColumns && this.filterableColumns.length > 0) {
      this.dataSource.filterPredicate = (data: T, filter: string) => {
        const searchTerm = filter.trim().toLowerCase();
        return this.filterableColumns.some(key => {
          const value = data[key as keyof T];
          return value?.toString().toLowerCase().includes(searchTerm);
        });
      };
    } else {
      this.dataSource.filterPredicate = (data: T, filter: string) => {
        const searchTerm = filter.trim().toLowerCase();
        return JSON.stringify(data).toLowerCase().includes(searchTerm);
      };
    }
  }

  private updateDisplayData() {
    if (this.serverSide) {
      this.filteredLength = this.totalCount;
      this.dataSource.data = this.data;
      if (this.table) {
        this.table.renderRows();
      }
      this.cdr.detectChanges();
      return;
    }

    const filterValue = this._filterValue.trim().toLowerCase();
    if (filterValue) {
      this.filteredData = this.allData.filter(item =>
        this.dataSource.filterPredicate(item, filterValue)
      );
    } else {
      this.filteredData = this.allData.slice();
    }

    this.filteredLength = this.filteredData.length;

    if (this.sort && this.sort.active) {
      const isAsc = this.sort.direction === 'asc';
      this.filteredData = this.filteredData.sort((a, b) => {
        const aValue = (a as any)[this.sort.active];
        const bValue = (b as any)[this.sort.active];
        if (aValue == null) return isAsc ? -1 : 1;
        if (bValue == null) return isAsc ? 1 : -1;
        if (typeof aValue === 'number' && typeof bValue === 'number') {
          return isAsc ? aValue - bValue : bValue - aValue;
        }
        const comparison = String(aValue).localeCompare(String(bValue));
        return isAsc ? comparison : -comparison;
      });
    }

    const start = this.pageIndex * this.pageSize;
    const end = start + this.pageSize;
    const pagedData = this.filteredData.slice(start, end);

    this.dataSource.data = pagedData;
    if (this.table) {
      this.table.renderRows();
    }
    this.cdr.detectChanges();
  }

  onPageChange(event: any) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.pageChange.emit({
      pageIndex: event.pageIndex,
      pageSize: event.pageSize,
    });
    if (!this.serverSide) {
      this.updateDisplayData();
    }
  }

  onSortChange(event: any) {
    this.sortChange.emit({
      active: event.active,
      direction: event.direction,
    });
    if (!this.serverSide) {
      this.pageIndex = 0;
      this.updateDisplayData();
    } else {
      this.pageChange.emit({ pageIndex: 0, pageSize: this.pageSize });
    }
  }

  showDivider(index: number): boolean {
    const actions = this.config.actions;
    if (!actions || index >= actions.length - 1) return false;
    const current = actions[index];
    const next = actions[index + 1];
    return !!current.group && !!next.group && current.group !== next.group;
  }

  onAction(action: TableAction<T>, row: T) {
    this.actionClick.emit({ action, row });
  }

  getColumnValue(row: T, column: TableColumn<T>): string {
    const value = row[column.key as keyof T];
    if (column.format) {
      return column.format(value, row);
    }
    return value as string;
  }

  get maxPageIndex(): number {
    const total = this.serverSide ? this.totalCount : this.filteredLength;
    return Math.ceil(total / this.pageSize) - 1;
  }

  goToPage(index: number): void {
    if (index >= 0 && index <= this.maxPageIndex && index !== this.pageIndex) {
      this.onPageChange({ pageIndex: index, pageSize: this.pageSize });
    }
  }

  get pages(): { page: number }[] {
    const total = this.maxPageIndex + 1;
    const current = this.pageIndex;
    const result: { page: number }[] = [];

    if (total <= 9) {
      for (let i = 0; i < total; i++) result.push({ page: i });
    } else {
      result.push({ page: 0 });
      if (current > 3) result.push({ page: -1 });
      const start = Math.max(1, current - 2);
      const end = Math.min(total - 2, current + 2);
      for (let i = start; i <= end; i++) result.push({ page: i });
      if (current < total - 4) result.push({ page: -1 });
      result.push({ page: total - 1 });
    }

    return result;
  }

  navigateToRow(id: number, allData: T[]): boolean {
    const index = allData.findIndex(item => (item as any).id === id);
    if (index === -1) return false;

    const page = Math.floor(index / this.pageSize);
    this.pageIndex = page;
    this.updateDisplayData();
    this._highlightId = id;

    setTimeout(() => {
      const row = document.querySelector(`[data-id="${id}"]`);
      const container = document.querySelector('mat-sidenav-content');
      if (row) {
        if (container) {
          const containerRect = container.getBoundingClientRect();
          const rowRect = row.getBoundingClientRect();
          const top = container.scrollTop + rowRect.top - containerRect.top - 80;
          container.scrollTo({ top, behavior: 'smooth' });
        } else {
          row.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
      }
    }, 100);

    setTimeout(() => {
      this._highlightId = null;
    }, 3000);

    return true;
  }
}
