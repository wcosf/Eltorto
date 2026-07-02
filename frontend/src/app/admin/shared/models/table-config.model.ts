import { Sort } from '@angular/material/sort';

export interface TableColumn<T = any> {
  key: string;
  label: string;
  sortable?: boolean;
  format?: (value: any, row: T) => string;
  template?: any;
  cssClass?: string;
}

export interface TableAction<T = any> {
  label: string;
  icon?: string;
  color?: 'primary' | 'accent' | 'warn';
  action: (row: T) => void;
  condition?: (row: T) => boolean;
}

export interface TableConfig<T = any> {
  columns: TableColumn<T>[];
  actions?: TableAction<T>[];
  pageSizeOptions?: number[];
  defaultPageSize?: number;
  enableSearch?: boolean;
  enableSort?: boolean;
}
