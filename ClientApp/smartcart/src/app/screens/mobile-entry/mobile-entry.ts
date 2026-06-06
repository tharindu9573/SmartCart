import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CartStateService } from '../../core/services/cart-state.service';

@Component({
  selector: 'app-mobile-entry',
  imports: [CommonModule],
  templateUrl: './mobile-entry.html',
  styleUrl: './mobile-entry.css',
})
export class MobileEntryComponent {
  digits = '';
  loading = false;
  error = '';

  readonly isMobile = typeof window !== 'undefined' &&
    window.matchMedia('(pointer: coarse)').matches;

  constructor(
    private router: Router,
    private api: ApiService,
    private state: CartStateService
  ) {}

  get formattedNumber(): string {
    return this.digits;
  }

  get fullMobile(): string {
    return `+94${this.digits}`;
  }

  press(key: string): void {
    if (key === 'del') {
      this.digits = this.digits.slice(0, -1);
    } else if (this.digits.length < 10) {
      this.digits += key;
    }
    this.error = '';
  }

  onNativeInput(event: Event): void {
    const val = (event.target as HTMLInputElement).value.replace(/\D/g, '').slice(0, 10);
    this.digits = val;
    (event.target as HTMLInputElement).value = val;
    this.error = '';
  }

  async send(): Promise<void> {
    if (this.digits.length < 9) { // Sri Lanka: 9 digits after +94
      this.error = 'Please enter a valid mobile number.';
      return;
    }
    this.loading = true;
    this.error = '';
    try {
      await this.api.sendOtp(this.fullMobile);
      this.state.mobileNumber.set(this.fullMobile);
      this.router.navigate(['/verify/otp']);
    } catch (err) {
      this.error = ApiService.extractError(err, 'Failed to send OTP. Please try again.');
    } finally {
      this.loading = false;
    }
  }

  back(): void {
    this.router.navigate(['/']);
  }
}
