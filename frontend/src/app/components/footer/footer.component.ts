import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, ContactSettings } from '../../services/api.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent implements OnInit {
  currentYear = new Date().getFullYear();
  contacts: ContactSettings | null = null;
  isLoading = true;
  error: string | null = null;

  workingHours = {
    weekdays: '10:00 - 20:00',
    saturday: '11:00 - 18:00',
    sunday: 'Выходной'
  };

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.loadContacts();
  }

  loadContacts(): void {
    this.isLoading = true;
    this.error = null;

    this.apiService.getContacts().subscribe({
      next: (data: ContactSettings) => {
        console.log('Contacts loaded:', data);
        this.contacts = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading contacts:', err);
        this.error = 'Не удалось загрузить контакты';
        this.isLoading = false;
      }
    });
  }

  getPhoneLink(phone: string): string {
    if (!phone) return '';
    const cleaned = phone.replace(/[^0-9+]/g, '');
    return `tel:${cleaned}`;
  }

  getMapLink(): string {
    if (this.contacts?.mapUrl) {
      return this.contacts.mapUrl;
    }
    if (this.contacts?.address) {
      return `https://yandex.ru/maps/?text=${encodeURIComponent(this.contacts.address)}`;
    }
    return '#';
  }

  formatPhone(phone: string): string {
    return phone;
  }
}
