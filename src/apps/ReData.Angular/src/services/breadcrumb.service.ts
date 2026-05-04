import {inject, Injectable, signal} from '@angular/core';
import {Breadcrumb} from '../types';
import {Router} from '@angular/router';
import {filter} from 'rxjs';
import {NavigationEnd} from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbsService {
  private router = inject(Router);
  private lastSegmentOverride: string | null = null;
  private breadcrumbsSource = signal<Breadcrumb[]>([]);

  constructor() {
    // Listen to navigation events and update breadcrumbs
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.updateBreadcrumbs();
    });
  }

  public path = this.breadcrumbsSource.asReadonly();

  private updateBreadcrumbs(): void {
    const tree = this.router.parseUrl(this.router.url);
    const primary = tree.root.children['primary'];
    const segments = primary?.segments.map(segment => segment.path) ?? [];
    let currentUrl = '';

    const breadcrumbs = segments.map((segment, index) => {
      currentUrl += `/${segment}`;

      // Apply override only to the last segment
      const isLastSegment = index === segments.length - 1;
      const label = isLastSegment && this.lastSegmentOverride
        ? this.lastSegmentOverride
        : segment;

      return <Breadcrumb>{
        label: label,
        url: currentUrl,
      };
    });

    // Clear override after use (on actual navigation)
    this.lastSegmentOverride = null;

    this.breadcrumbsSource.set([...breadcrumbs]);
  }

  /**
   * Set override for the last segment of the current breadcrumb
   * @param label The display label to show instead of the last segment
   */
  setLastSegment(label: string): void {
    this.lastSegmentOverride = label;
    this.updateBreadcrumbs(); // Manually update breadcrumbs
  }
}
