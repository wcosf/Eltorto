import { Component, Input, Output, EventEmitter, OnInit, AfterViewInit, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
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
  @Input() filterValue: string = ''; // внешний поиск

  @Output() pageChange = new EventEmitter<{ pageIndex: number; pageSize: number }>();
  @Output() sortChange = new EventEmitter<{ active: string; direction: 'asc' | 'desc' }>();
  @Output() actionClick = new EventEmitter<{ action: TableAction<T>; row: T }>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = [];
  dataSource = new MatTableDataSource<T>([]);

  ngOnInit() {
    this.initTable();
    this.setupFilter();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    this.updateDataSource();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['data'] || changes['totalCount'] || changes['pageSize'] || changes['pageIndex']) {
      this.updateDataSource();
    }
    if (changes['filterValue']) {
      this.applyFilter();
    }
  }

  private initTable() {
    this.displayedColumns = this.config.columns.map(c => c.key as string);
    if (this.config.actions?.length) {
      this.displayedColumns.push('actions');
    }
    this.dataSource = new MatTableDataSource(this.data);
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

  private updateDataSource() {
    this.dataSource.data = this.data;
    if (this.paginator) {
      this.paginator.length = this.totalCount;
      this.paginator.pageIndex = this.pageIndex;
      this.paginator.pageSize = this.pageSize;
    }
    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
    this.applyFilter();
  }

  private applyFilter() {
    this.dataSource.filter = this.filterValue.trim().toLowerCase();
    if (this.paginator) {
      this.paginator.firstPage();
    }
  }

  onPageChange(event: any) {
    this.pageChange.emit({
      pageIndex: event.pageIndex,
      pageSize: event.pageSize,
    });
  }

  onSortChange(event: any) {
    this.sortChange.emit({
      active: event.active,
      direction: event.direction,
    });
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
