import { Injectable } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';


@Injectable({
  providedIn: 'root',
})
export class VisitorIdService {
  private readonly COOKIE_NAME = 'visitorId';
  private readonly COOKIE_EXPIRY_DAYS = 365; // 1 year
  private visitorId: string | null = null;

  constructor(
    private cookieService: CookieService,
  ) {
    this.initializeVisitorId();
  }

  /**
   * Initialize or retrieve visitor ID
   */
  private initializeVisitorId(): void {
    this.visitorId = this.getVisitorIdFromCookie();

    if (!this.visitorId) {
      this.visitorId = this.generateVisitorId();
      this.setVisitorIdCookie(this.visitorId);
      console.log('new visitor:', this.visitorId);
    } else {
      console.log('old visitor:', this.visitorId);
    }
  }

  /**
   * Get visitor ID from cookie or generate new one
   */
  private getVisitorIdFromCookie(): string | null {
    try {
      const id = this.cookieService.get(this.COOKIE_NAME);
      return id || null;
    } catch (error) {
      console.error('Error reading visitorId cookie:', error);
      return null;
    }
  }

  /**
   * Generate a random visitor ID
   */
  private generateVisitorId(): string {
    // Generate UUID v4
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

  /**
   * Set visitor ID cookie
   */
  private setVisitorIdCookie(visitorId: string): void {
    try {
      const expiryDate = new Date();
      expiryDate.setDate(expiryDate.getDate() + this.COOKIE_EXPIRY_DAYS);

      this.cookieService.set(
        this.COOKIE_NAME,
        visitorId,
        expiryDate,
        '/', // path
        '', // domain (current domain)
        false, // secure
        'Lax' // sameSite
      );
    } catch (error) {
      console.error('Error setting visitorId cookie:', error);
    }
  }

  /**
   * Get current visitor ID
   */
  getVisitorId(): string | null {
    return this.visitorId;
  }
}
