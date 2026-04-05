import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({
  name: 'sanitizeHtml',
  standalone: true
})
export class SanitizeHtmlPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}

  transform(value: string | null | undefined): SafeHtml {
    if (!value) return '';

    const allowedTags = ['p', 'br', 'strong', 'b', 'em', 'i', 'u', 'a', 'span'];

    const cleaned = this.sanitizeHtml(value, allowedTags);

    return this.sanitizer.sanitize(1, cleaned) || '';
  }

  private sanitizeHtml(html: string, allowedTags: string[]): string {
    const temp = document.createElement('div');
    temp.innerHTML = html;

    const allElements = temp.getElementsByTagName('*');
    for (let i = allElements.length - 1; i >= 0; i--) {
      const element = allElements[i];
      if (!allowedTags.includes(element.tagName.toLowerCase())) {
        element.outerHTML = element.textContent || '';
      } else {
        this.removeDangerousAttributes(element);
      }
    }

    return temp.innerHTML;
  }

  private removeDangerousAttributes(element: Element): void {
    const dangerousAttrs = ['onclick', 'onload', 'onerror', 'onmouseover', 'javascript:'];
    const attributes = element.attributes;

    for (let i = attributes.length - 1; i >= 0; i--) {
      const attr = attributes[i];
      const attrName = attr.name.toLowerCase();
      const attrValue = attr.value.toLowerCase();

      if (dangerousAttrs.some(danger => attrName.includes(danger) || attrValue.includes(danger))) {
        element.removeAttribute(attr.name);
      }
    }
  }
}
