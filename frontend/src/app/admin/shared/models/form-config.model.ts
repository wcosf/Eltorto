import { ValidatorFn, AsyncValidatorFn } from '@angular/forms';

export type FormFieldType =
  | 'text'
  | 'textarea'
  | 'number'
  | 'select'
  | 'checkbox'
  | 'file'
  | 'date'
  | 'email';

export interface FormFieldOption {
  value: any;
  label: string;
}

export interface FormField {
  key: string;
  label: string;
  type: FormFieldType;
  required?: boolean;
  validators?: ValidatorFn[];
  asyncValidators?: AsyncValidatorFn[];
  options?: FormFieldOption[];
  placeholder?: string;
  defaultValue?: any;
  hidden?: boolean;
  disabled?: boolean;
  fileAccept?: string;
  rows?: number;
  hint?: string;
}

export interface FormConfig {
  title: string;
  fields: FormField[];
  submitLabel?: string;
  cancelLabel?: string;
  initialValue?: Record<string, any>;
}
