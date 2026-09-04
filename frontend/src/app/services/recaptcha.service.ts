import { Injectable } from '@angular/core';
import { ConfigService } from './config.service';

declare const grecaptcha: any;

@Injectable({
  providedIn: 'root'
})
export class RecaptchaService {
  private scriptLoaded = false;

  constructor(private configService: ConfigService) {}

  private ensureScript(): Promise<void> {
    if (this.scriptLoaded && typeof grecaptcha !== 'undefined') {
      return Promise.resolve();
    }

    return new Promise<void>((resolve, reject) => {
      const siteKey = this.configService.recaptchaSiteKey;
      if (!siteKey) {
        reject('reCAPTCHA site key not configured');
        return;
      }

      const script = document.createElement('script');
      script.src = `https://www.google.com/recaptcha/api.js?render=${siteKey}`;
      script.async = true;
      script.defer = true;
      script.onload = () => {
        this.scriptLoaded = true;
        resolve();
      };
      script.onerror = () => reject('Failed to load reCAPTCHA script');
      document.head.appendChild(script);
    });
  }

  async getToken(action: string = 'submit'): Promise<string> {
    await this.ensureScript();

    return new Promise<string>((resolve, reject) => {
      grecaptcha.ready(() => {
        grecaptcha.execute(this.configService.recaptchaSiteKey, { action }).then(
          (token: string) => resolve(token),
          (error: any) => reject(error)
        );
      });
    });
  }
}
