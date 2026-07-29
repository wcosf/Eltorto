import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { Subject, Observable, finalize } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { FormConfig, FormField, FormFieldOption } from '../../models/form-config.model';

export interface FormModalData {
  config: FormConfig;
  initialValue?: any;
}

@Component({
  selector: 'app-form-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  templateUrl: './form-modal.component.html',
  styleUrls: ['./form-modal.component.scss']
})
export class FormModalComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  loading = false;
  config: FormConfig;
  fields: FormField[];
  error: string | null = null;
  selectedFile: File | null = null;
  filteredOptionsMap = new Map<string, FormFieldOption[]>();
  asyncLoadingMap = new Map<string, boolean>();
  private displayFnMap = new Map<string, (value: any) => string>();
  private destroy$ = new Subject<void>();

  constructor(
    public dialogRef: MatDialogRef<FormModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: FormModalData,
    private fb: FormBuilder
  ) {
    this.config = data.config;
    this.fields = this.config.fields;
  }

  ngOnInit() {
    this.buildForm();
    this.setInitialValue();
    this.initDisplayFns();
    this.setupAutocompleteFilters();
  }

  private setInitialValue(): void {
    this.fields.forEach(field => {
      const initial = this.data.initialValue || this.config.initialValue || {};
      const value = initial[field.key] !== undefined ? initial[field.key] : field.defaultValue ?? '';
      this.form.get(field.key)?.setValue(value, { emitEvent: false });
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildForm() {
    const group: any = {};
    const initial = this.data.initialValue || this.config.initialValue || {};

    this.fields.forEach(field => {
      const value = initial[field.key] !== undefined ? initial[field.key] : field.defaultValue ?? '';
      const validators: ValidatorFn[] = field.required ? [Validators.required] : [];
      if (field.validators) {
        validators.push(...field.validators);
      }
      const controlState = field.disabled ? { value, disabled: true } : value;
      group[field.key] = [controlState, validators];
    });

    this.form = this.fb.group(group);
  }

  private setupAutocompleteFilters() {
    this.fields.filter(f => f.type === 'autocomplete').forEach(field => {
      const control = this.form.get(field.key);
      if (!control) return;

      if (field.asyncOptionsFn) {
        this.filteredOptionsMap.set(field.key, []);
        control.valueChanges
          .pipe(
            takeUntil(this.destroy$),
            debounceTime(300),
            distinctUntilChanged(),
            switchMap(value => {
              const query = typeof value === 'string' ? value : '';
              this.asyncLoadingMap.set(field.key, true);
              return field.asyncOptionsFn!(query);
            })
          )
          .subscribe(result => {
            this.filteredOptionsMap.set(field.key, result);
            this.asyncLoadingMap.set(field.key, false);
          });
      } else {
        this.filteredOptionsMap.set(field.key, field.options || []);
        control.valueChanges
          .pipe(takeUntil(this.destroy$), debounceTime(200))
          .subscribe(value => {
            this.updateFilteredOptions(field, value);
          });
      }
    });
  }

  private updateFilteredOptions(field: FormField, value: any) {
    if (field.asyncOptionsFn) return;
    const filterText = typeof value === 'string' ? value.toLowerCase() : '';
    if (!filterText) {
      this.filteredOptionsMap.set(field.key, field.options || []);
    } else {
      const filtered = (field.options || []).filter(opt =>
        opt.label.toLowerCase().includes(filterText)
      );
      this.filteredOptionsMap.set(field.key, filtered);
    }
  }

  getFilteredOptions(field: FormField): FormFieldOption[] {
    return this.filteredOptionsMap.get(field.key) || field.options || [];
  }

  private initDisplayFns() {
    this.fields.filter(f => f.type === 'autocomplete').forEach(field => {
      if (field.displayFn) {
        this.displayFnMap.set(field.key, field.displayFn);
      } else {
        this.displayFnMap.set(field.key, (value: any) => {
          if (!value) return '';
          const option = field.options?.find(o => o.value === value);
          if (option) return option.label;
          return String(value);
        });
      }
    });
  }

  getDisplayFn(field: FormField): (value: any) => string {
    return this.displayFnMap.get(field.key) || ((v: any) => v ?? '');
  }

  getSelectedOption(field: FormField): FormFieldOption | undefined {
    const value = this.form?.get(field.key)?.value;
    if (!value) return undefined;

    const options = field.asyncOptionsFn
      ? this.filteredOptionsMap.get(field.key)
      : field.options;
    if (!options) return undefined;

    // Compare by id for objects with id property, otherwise by reference
    const valueId = typeof value === 'object' && value.id !== undefined ? value.id : value;
    return options.find(o => {
      const optValue = o.value;
      const optId = typeof optValue === 'object' && optValue.id !== undefined ? optValue.id : optValue;
      return optId === valueId;
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.error = null;
    this.loading = true;
    const result = { ...this.form.value, _file: this.selectedFile };
    this.dialogRef.close(result);
  }

  onCancel() {
    this.dialogRef.close();
  }

  onFileSelected(event: Event, key: string): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.selectedFile = file;
      this.form.patchValue({ [key]: file });
    }
  }

  isRequired(field: FormField): boolean {
    return !!field.required;
  }

  getControlErrorMessage(field: FormField): string | null {
    const control = this.form.get(field.key);
    if (!control || !control.errors || !control.touched) return null;

    const errors = control.errors;
    const msg = field.validationMessages;

    if (errors['required']) return `${field.label} обязательно`;
    if (errors['minlength']) return `Минимальная длина: ${errors['minlength'].requiredLength} симв.`;
    if (errors['maxlength']) return `Максимальная длина: ${errors['maxlength'].requiredLength} симв.`;
    if (errors['email']) return msg?.['email'] || 'Некорректный формат email';
    if (errors['pattern']) return msg?.['pattern'] || 'Неверный формат';
    if (errors['min']) return `Минимальное значение: ${errors['min'].min}`;
    if (errors['max']) return `Максимальное значение: ${errors['max'].max}`;
    if (errors['pastDate']) return msg?.['pastDate'] || 'Дата не может быть в прошлом';

    const firstKey = Object.keys(errors)[0];
    if (firstKey && msg?.[firstKey]) return msg[firstKey];

    return null;
  }
}
