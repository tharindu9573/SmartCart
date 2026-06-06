import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  OtpVerifyResponse,
  UpsertUserResponse,
  PaymentCard,
  PaymentResult,
} from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** Extracts a human-readable message from an API error response. */
  static extractError(err: unknown, fallback = 'Something went wrong. Please try again.'): string {
    if (err instanceof HttpErrorResponse) {
      return err.error?.message ?? fallback;
    }
    return fallback;
  }

  // ── User ──────────────────────────────────────────────────────────────────
  sendOtp(mobileNumber: string) {
    return firstValueFrom(
      this.http.post<{ message: string }>(`${this.base}/api/user/send-otp`, { mobileNumber })
    );
  }

  verifyOtp(mobileNumber: string, otp: string) {
    return firstValueFrom(
      this.http.post<OtpVerifyResponse>(`${this.base}/api/user/verify-otp`, { mobileNumber, otp })
    );
  }

  upsertUser(mobileNumber: string, name: string, email: string) {
    return firstValueFrom(
      this.http.post<UpsertUserResponse>(`${this.base}/api/user/upsert`, { mobileNumber, name, email })
    );
  }

  // ── Cart Session ──────────────────────────────────────────────────────────
  startSession(cartId: number, userId: number) {
    return firstValueFrom(
      this.http.post<{ sessionId: number; status: string }>(
        `${this.base}/api/cartsession/start`,
        { cartId, userId }
      )
    );
  }

  confirmCart(sessionId: number, uids: string[]) {
    return firstValueFrom(
      this.http.post<{ message: string; status: string }>(
        `${this.base}/api/cartsession/${sessionId}/confirm`,
        { uids }
      )
    );
  }

  getActiveSession(cartId: number) {
    return firstValueFrom(
      this.http.get<{ sessionId: number; status: string; items: any[] }>(
        `${this.base}/api/cartsession/active/${cartId}`
      )
    );
  }

  resetSession(cartId: number) {
    return firstValueFrom(
      this.http.post<void>(`${this.base}/api/cartsession/${cartId}/reset`, {})
    );
  }

  // ── Payment ───────────────────────────────────────────────────────────────
  resolveToken(paymentUid: string) {
    return firstValueFrom(
      this.http.post<PaymentCard>(`${this.base}/api/payment/resolve-token`, { paymentUid })
    );
  }

  processPayment(sessionId: number, paymentMethod: string, paymentToken: string, amount: number) {
    return firstValueFrom(
      this.http.post<PaymentResult>(`${this.base}/api/payment/process`, {
        sessionId,
        paymentMethod,
        paymentToken,
        amount,
      })
    );
  }
}
