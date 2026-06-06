import { Component, OnInit, OnDestroy, AfterViewInit, ElementRef, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CartStateService } from '../../core/services/cart-state.service';

@Component({
  selector: 'app-otp-verify',
  imports: [CommonModule],
  templateUrl: './otp-verify.html',
  styleUrl: './otp-verify.css',
})
export class OtpVerifyComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('otpInput') otpInputRef?: ElementRef<HTMLInputElement>;

  digits = '';
  loading = false;
  error = '';
  readonly secondsLeft = signal(300);
  private timerRef: ReturnType<typeof setInterval> | null = null;

  readonly isMobile = typeof window !== 'undefined' &&
    window.matchMedia('(pointer: coarse)').matches;

  constructor(
    private router: Router,
    private api: ApiService,
    private state: CartStateService
  ) {}

  get mobile(): string { return this.state.mobileNumber(); }
  get maskedMobile(): string {
    const m = this.mobile;
    return m.length > 4 ? m.slice(0, -4).replace(/./g, '*') + m.slice(-4) : m;
  }
  get timerLabel(): string {
    const secs = this.secondsLeft();
    const m = Math.floor(secs / 60).toString().padStart(2, '0');
    const s = (secs % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  }
  get boxes(): string[] {
    return [0, 1, 2, 3, 4, 5].map(i => this.digits[i] || '');
  }

  ngOnInit(): void {
    if (!this.mobile) { this.router.navigate(['/verify/mobile']); return; }
    this.startTimer();
  }

  ngAfterViewInit(): void {
    if (this.isMobile) {
      setTimeout(() => this.otpInputRef?.nativeElement.focus(), 100);
    }
  }

  ngOnDestroy(): void { this.clearTimer(); }

  private startTimer(): void {
    this.secondsLeft.set(300);
    this.timerRef = setInterval(() => {
      this.secondsLeft.update(s => s - 1);
      if (this.secondsLeft() <= 0) this.clearTimer();
    }, 1000);
  }

  private clearTimer(): void {
    if (this.timerRef) { clearInterval(this.timerRef); this.timerRef = null; }
  }

  press(key: string): void {
    if (key === 'del') {
      this.digits = this.digits.slice(0, -1);
    } else if (this.digits.length < 6) {
      this.digits += key;
      if (this.digits.length === 6) this.verify();
    }
    this.error = '';
  }

  focusOtpInput(): void {
    this.otpInputRef?.nativeElement.focus();
  }

  onNativeOtpInput(event: Event): void {
    const val = (event.target as HTMLInputElement).value.replace(/\D/g, '').slice(0, 6);
    this.digits = val;
    (event.target as HTMLInputElement).value = val;
    this.error = '';
    if (val.length === 6) this.verify();
  }

  async verify(): Promise<void> {
    if (this.digits.length !== 6) return;
    this.loading = true;
    this.error = '';
    try {
      const res = await this.api.verifyOtp(this.mobile, this.digits);
      if (res.exists && res.userId && res.name && res.email) {
        this.state.user.set({ userId: res.userId, name: res.name, email: res.email, mobile: this.mobile });
        this.router.navigate(['/verify/confirm']);
      } else {
        this.router.navigate(['/verify/setup']);
      }
    } catch (err) {
      this.error = ApiService.extractError(err, 'Invalid or expired code. Please try again.');
      this.digits = '';
    } finally {
      this.loading = false;
    }
  }

  async resend(): Promise<void> {
    this.digits = '';
    this.error = '';
    this.clearTimer();
    try {
      await this.api.sendOtp(this.mobile);
      this.startTimer();
    } catch (err) {
      this.error = ApiService.extractError(err, 'Could not resend OTP. Please try again.');
    }
  }

  back(): void { this.router.navigate(['/verify/mobile']); }
}
