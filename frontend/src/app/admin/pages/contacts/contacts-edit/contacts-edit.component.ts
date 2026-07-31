import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { finalize } from 'rxjs/operators';

import { AdminNotificationService } from '../../../shared/services/admin-notification.service';
import { ApiService, ContactSettings } from '../../../../services/api.service';

@Component({
  selector: 'app-contacts-edit',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  templateUrl: './contacts-edit.component.html',
  styleUrl: './contacts-edit.component.scss'
})
export class ContactsEditComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private notification: AdminNotificationService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadContacts();
  }

  private initForm(): void {
    this.form = this.fb.group({
      phone: ['', [Validators.required, Validators.pattern(/^\+?[\d\s\-()]{7,20}$/)]],
      email: ['', [Validators.required, Validators.email]],
      address: [''],
    });
  }

  private loadContacts(): void {
    this.loading = true;
    this.apiService.getContacts()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (data: ContactSettings) => this.form.patchValue(data),
        error: () => this.notification.error('Не удалось загрузить настройки контактов')
      });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.saving = true;
    this.apiService.updateContacts(this.form.value)
      .pipe(finalize(() => this.saving = false))
      .subscribe({
        next: (data: ContactSettings) => {
          this.form.patchValue(data);
          this.form.markAsPristine();
          this.notification.success('Настройки контактов сохранены');
        },
        error: () => this.notification.error('Не удалось сохранить настройки контактов')
      });
  }

  get f() {
    return this.form.controls;
  }
}
