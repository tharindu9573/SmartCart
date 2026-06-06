import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenResponse } from '../models';

const ACCESS_TOKEN_KEY = 'sc_access_token';
const REFRESH_TOKEN_KEY = 'sc_refresh_token';
const EXPIRES_AT_KEY = 'sc_expires_at';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenUrl = `${environment.apiUrl}/api/auth/token`;
  private readonly refreshUrl = `${environment.apiUrl}/api/auth/refresh`;

  constructor(private http: HttpClient) {}

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  isTokenExpired(): boolean {
    const expiresAt = localStorage.getItem(EXPIRES_AT_KEY);
    if (!expiresAt) return true;
    return new Date(expiresAt) <= new Date(Date.now() + 30_000); // 30s buffer
  }

  async ensureAuthenticated(): Promise<string> {
    if (this.getAccessToken() && !this.isTokenExpired()) {
      return this.getAccessToken()!;
    }
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      try {
        return await this.refresh(refreshToken);
      } catch {
        // Fall through to full auth
      }
    }
    return await this.authenticate();
  }

  async authenticate(): Promise<string> {
    const result = await firstValueFrom(
      this.http.post<TokenResponse>(this.tokenUrl, {
        cartId: environment.cartId,
        clientSecret: environment.clientSecret,
      })
    );
    this.storeTokens(result);
    return result.accessToken;
  }

  async refresh(refreshToken: string): Promise<string> {
    const result = await firstValueFrom(
      this.http.post<TokenResponse>(this.refreshUrl, { refreshToken })
    );
    this.storeTokens(result);
    return result.accessToken;
  }

  private storeTokens(response: TokenResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(EXPIRES_AT_KEY, response.expiresAt);
  }

  clearTokens(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(EXPIRES_AT_KEY);
  }
}
