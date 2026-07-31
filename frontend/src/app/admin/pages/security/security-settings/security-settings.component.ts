import { Component, ViewChild, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormGroupDirective } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { AuthService } from '../../../../core/auth.service';
import { AdminNotificationService } from '../../../shared/services/admin-notification.service';

export function passwordMatchValidator(form: FormGroup) {
  const password = form.get('newPassword')?.value;
  const confirm = form.get('confirmNewPassword')?.value;
  return password === confirm ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-security-settings',
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
  templateUrl: './security-settings.component.html',
  styleUrl: './security-settings.component.scss'
})
export class SecuritySettingsComponent {
  passwordForm: FormGroup;
  loginForm: FormGroup;
  savingPassword = false;
  savingLogin = false;

  @ViewChild('passwordFormDirective') private passwordFormDirective!: FormGroupDirective;
  @ViewChild('loginFormDirective') private loginFormDirective!: FormGroupDirective;

  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmNewPassword = true;
  hidePasswordLogin = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notification: AdminNotificationService
  ) {
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$/)]],
      confirmNewPassword: ['', Validators.required],
    }, { validators: passwordMatchValidator });

    this.loginForm = this.fb.group({
      newUserName: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', Validators.required],
    });
  }

  onChangePassword(): void {
    if (this.passwordForm.invalid) return;

    this.savingPassword = true;
    this.authService.changePassword({
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword,
    }).subscribe({
      next: () => {
        this.notification.success('Пароль успешно изменён');
        this.passwordForm.reset();
        this.passwordForm.markAsPristine();
        this.passwordFormDirective.resetForm();
        this.savingPassword = false;
      },
      error: (err) => {
        const message = err.error?.error || 'Не удалось сменить пароль';
        this.notification.error(message);
        this.savingPassword = false;
      }
    });
  }

  onChangeLogin(): void {
    if (this.loginForm.invalid) return;

    this.savingLogin = true;
    this.authService.changeUserName({
      newUserName: this.loginForm.value.newUserName,
      password: this.loginForm.value.password,
    }).subscribe({
      next: () => {
        this.notification.success('Логин успешно изменён');
        this.loginForm.reset();
        this.loginForm.markAsPristine();
        this.loginFormDirective.resetForm();
        this.savingLogin = false;
      },
      error: (err) => {
        const message = err.error?.error || 'Не удалось сменить логин';
        this.notification.error(message);
        this.savingLogin = false;
      }
    });
  }

  get pf() {
    return this.passwordForm.controls;
  }

  get lf() {
    return this.loginForm.controls;
  }
}
