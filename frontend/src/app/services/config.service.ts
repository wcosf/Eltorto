import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

interface ClientConfig {
  recaptchaSiteKey: string;
}

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private config: ClientConfig | null = null;

  constructor(private http: HttpClient) {}

  async load(): Promise<void> {
    if (this.config) return;
    this.config = await firstValueFrom(this.http.get<ClientConfig>('/api/config/client'));
  }

  get recaptchaSiteKey(): string {
    return this.config?.recaptchaSiteKey ?? '';
  }
}
