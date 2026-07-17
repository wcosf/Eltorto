import {
  Component, Input, Output, EventEmitter, OnInit, AfterViewInit,
  OnChanges, SimpleChanges, ViewChild, TemplateRef, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
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
    MatPaginatorModule,
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
  @Input() pageSize = 10;
  @Input() pageIndex = 0;
  @Input() config!: TableConfig<T>;
  @Input() loading = false;
  @Input() filterableColumns: string[] = [];
  @Input() columnTemplates: { [key: string]: TemplateRef<any> } = {};
  @Input() defaultSort?: { active: string; direction: 'asc' | 'desc' };

  @Output() pageChange = new EventEmitter<{ pageIndex: number; pageSize: number }>();
  @Output() sortChange = new EventEmitter<{ active: string; direction: 'asc' | 'desc' }>();
  @Output() actionClick = new EventEmitter<{ action: TableAction<T>; row: T }>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private _filterValue = '';
  @Input() set filterValue(value: string) {
    const newValue = value || '';
    if (this._filterValue !== newValue) {
      this._filterValue = newValue;
      this.updateDisplayData();
      if (this.paginator) {
        this.paginator.firstPage();
      }
    }
  }
  get filterValue(): string {
    return this._filterValue;
  }

  constructor(private cdr: ChangeDetectorRef) {}

  displayedColumns: string[] = [];
  dataSource = new MatTableDataSource<T>([]);

  private allData: T[] = [];
  private filteredData: T[] = [];
  private currentPageData: T[] = [];

  ngOnInit() {
    this.initTable();
    this.setupFilter();
    this.allData = this.data;
    this.updateDisplayData();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
    if (this.defaultSort && this.sort) {
      this.sort.active = this.defaultSort.active;
      this.sort.direction = this.defaultSort.direction;
      this.sort.sortChange.emit({ active: this.defaultSort.active, direction: this.defaultSort.direction });
    }
    this.updateDisplayData();
  }

  ngOnChanges(changes: SimpleChanges) {
    console.log('DataTable ngOnChanges:', Object.keys(changes));
    if (changes['data']) {
      this.allData = this.data;
      this.updateDisplayData();
    }
    if (changes['pageSize'] || changes['pageIndex']) {
      this.updateDisplayData();
    }
    if (changes['totalCount']) {
      this.updatePaginatorLength();
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
    const filterValue = this._filterValue.trim().toLowerCase();
    if (filterValue) {
      this.filteredData = this.allData.filter(item =>
        this.dataSource.filterPredicate(item, filterValue)
      );
    } else {
      this.filteredData = this.allData.slice();
    }

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
    this.currentPageData = this.filteredData.slice(start, end);

    this.dataSource.data = this.currentPageData;

    this.updatePaginatorLength();
    this.cdr.detectChanges();
  }

  private updatePaginatorLength() {
    if (this.paginator) {
      this.paginator.length = this.filteredData.length;
      this.paginator.pageIndex = this.pageIndex;
      this.paginator.pageSize = this.pageSize;
    }
  }

  onPageChange(event: any) {
    console.log('DataTable onPageChange:', event);
    this.pageChange.emit({
      pageIndex: event.pageIndex,
      pageSize: event.pageSize,
    });
  }

  onSortChange(event: any) {
    console.log('onSortChange:', event);
    this.sortChange.emit({
      active: event.active,
      direction: event.direction,
    });
    this.updateDisplayData();
    if (this.paginator) {
      this.paginator.firstPage();
    }
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
}
