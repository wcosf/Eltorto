import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { TableConfig, TableAction, TableColumn } from '../../models/table-config.model';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss']
})
export class DataTableComponent<T> implements OnInit, OnChanges {
  @Input() data: T[] = [];
  @Input() totalCount = 0;
  @Input() pageSize = 10;
  @Input() pageIndex = 0;
  @Input() config!: TableConfig<T>;
  @Input() loading = false;
  @Input() filterableColumns: string[] = [];

  @Output() pageChange = new EventEmitter<{ pageIndex: number; pageSize: number }>();
  @Output() sortChange = new EventEmitter<{ active: string; direction: 'asc' | 'desc' }>();
  @Output() searchChange = new EventEmitter<string>();
  @Output() actionClick = new EventEmitter<{ action: TableAction<T>; row: T }>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = [];
  dataSource = new MatTableDataSource<T>([]);
  searchTerm = '';

  ngOnInit() {
    this.initTable();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['data'] || changes['totalCount'] || changes['pageSize'] || changes['pageIndex']) {
      this.updateDataSource();
    }
  }

  private initTable() {
    this.displayedColumns = this.config.columns.map(c => c.key as string);
    if (this.config.actions?.length) {
      this.displayedColumns.push('actions');
    }
    this.dataSource = new MatTableDataSource(this.data);
    this.dataSource.sort = this.sort;
  }

  private updateDataSource() {
    this.dataSource.data = this.data;
    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
    if (this.paginator) {
      this.paginator.length = this.totalCount;
      this.paginator.pageIndex = this.pageIndex;
      this.paginator.pageSize = this.pageSize;
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

  onSearch() {
    this.searchChange.emit(this.searchTerm);
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
