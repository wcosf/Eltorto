import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { FormConfig, FormField } from '../../models/form-config.model';

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
export class FormModalComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  config: FormConfig;
  fields: FormField[];

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
  }

  private buildForm() {
    const group: any = {};
    const initial = this.data.initialValue || this.config.initialValue || {};

    this.fields.forEach(field => {
      const value = initial[field.key] !== undefined ? initial[field.key] : field.defaultValue || '';
      const validators = field.required ? [Validators.required] : [];
      if (field.validators) {
        validators.push(...field.validators);
      }
      group[field.key] = [value, validators];
    });

    this.form = this.fb.group(group);
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.dialogRef.close(this.form.value);
  }

  onCancel() {
    this.dialogRef.close();
  }

  onFileSelected(event: Event, key: string): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.form.patchValue({ [key]: file });
    }
  }

  isRequired(field: FormField): boolean {
    return !!field.required;
  }
}
