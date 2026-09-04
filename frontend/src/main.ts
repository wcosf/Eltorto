import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './app/interceptors/auth.interceptor';
import { RefreshTokenInterceptor } from './app/interceptors/refresh-token.interceptor';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideToastr } from 'ngx-toastr';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { RussianPaginatorIntl } from './app/core/russian-paginator-intl';
import { ConfigService } from './app/services/config.service';
import { APP_INITIALIZER } from '@angular/core';

export function initializeConfig(configService: ConfigService) {
  return () => configService.load();
}

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideAnimations(),
    provideToastr({
      positionClass: 'toast-top-right',
      timeOut: 5000,
      closeButton: true,
      progressBar: true,
    }),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: RefreshTokenInterceptor, multi: true },
    { provide: MatPaginatorIntl, useClass: RussianPaginatorIntl },
    ConfigService,
    { provide: APP_INITIALIZER, useFactory: initializeConfig, deps: [ConfigService], multi: true }
  ]
}).catch(err => console.error(err));
