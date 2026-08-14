import { Component, HostListener, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-scroll-to-top',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './scroll-to-top.component.html',
  styleUrls: ['./scroll-to-top.component.scss']
})
export class ScrollToTopComponent {
  readonly showButton = signal(false);

  @Input() scrollTarget?: HTMLElement;

  @HostListener('window:scroll')
  onWindowScroll(): void {
    if (!this.scrollTarget) {
      this.updateVisibility(window.scrollY || document.documentElement.scrollTop);
    }
  }

  onContainerScroll(event: Event): void {
    this.updateVisibility((event.target as HTMLElement).scrollTop);
  }

  scrollToTop(): void {
    if (this.scrollTarget) {
      this.scrollTarget.scrollTo({ top: 0, left: 0, behavior: 'smooth' });
    } else {
      window.scrollTo({ top: 0, left: 0, behavior: 'smooth' });
    }
  }

  private updateVisibility(scrollPosition: number): void {
    this.showButton.set(scrollPosition > 350);
  }
}
