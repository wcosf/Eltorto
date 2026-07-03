import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})

export class AdminNotificationService {
  constructor(private toastr: ToastrService) {}

  success(message: string): void {
    this.toastr.success(message, 'Успех');
  }

  error(message: string): void {
    this.toastr.error(message, 'Ошибка');
  }

  warning(message: string): void {
    this.toastr.warning(message, 'Внимание');
  }

  info(message: string): void {
    this.toastr.info(message, 'Информация');
  }
}
