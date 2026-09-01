import { Sort } from '@angular/material/sort';

export interface TableColumn<T = any> {
  key: string;
  label: string;
  sortable?: boolean;
  format?: (value: any, row: T) => string;
  html?: boolean;
  template?: any;
  cssClass?: string;
  sticky?: boolean;
}

export interface TableAction<T = any> {
  label: string | ((row: T) => string);
  icon?: string | ((row: T) => string);
  color?: 'primary' | 'accent' | 'warn' | ((row: T) => 'primary' | 'accent' | 'warn');
  action: (row: T) => void;
  condition?: (row: T) => boolean;
  group?: string;
  cssClass?: string | ((row: T) => string);
}

export interface TableConfig<T = any> {
  columns: TableColumn<T>[];
  actions?: TableAction<T>[];
  pageSizeOptions?: number[];
  defaultPageSize?: number;
  enableSearch?: boolean;
  enableSort?: boolean;
}
