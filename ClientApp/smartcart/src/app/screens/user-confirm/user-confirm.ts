import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CartStateService } from '../../core/services/cart-state.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-user-confirm',
  imports: [CommonModule],
  templateUrl: './user-confirm.html',
  styleUrl: './user-confirm.css',
})
export class UserConfirmComponent implements OnInit {
  loading = false;
  error = '';

  constructor(
    private router: Router,
    private api: ApiService,
    private state: CartStateService
  ) {}

  get user() { return this.state.user(); }

  ngOnInit(): void {
    if (!this.user) { this.router.navigate(['/verify/mobile']); }
  }

  async confirm(): Promise<void> {
    if (!this.user) return;
    this.loading = true;
    try {
      let sessionId: number;
      let status: string;
      let existingItems: any[] | null = null;

      try {
        const active = await this.api.getActiveSession(environment.cartId);
        sessionId = active.sessionId;
        status = active.status;
        existingItems = active.items ?? [];
      } catch {
        const session = await this.api.startSession(environment.cartId, this.user.userId);
        sessionId = session.sessionId;
        status = session.status;
      }

      this.state.session.set({ sessionId, cartId: environment.cartId, status });

      if (existingItems && existingItems.length > 0) {
        this.state.items.set(
          existingItems.map((i: any) => ({
            uids: Array.from({ length: i.quantity }, (_: any, idx: number) => `${i.productId}-restored-${idx}`),
            productId: i.productId,
            name: i.name,
            price: i.price,
            imageUrl: i.imageUrl ?? null,
            categoryName: i.categoryName ?? '',
            quantity: i.quantity,
          }))
        );
      }

      this.router.navigate(['/cart']);
    } catch (err) {
      this.error = ApiService.extractError(err, 'Could not start session. Please try again.');
    } finally {
      this.loading = false;
    }
  }

  notMe(): void {
    this.state.user.set(null);
    this.router.navigate(['/verify/setup']);
  }

  back(): void { this.router.navigate(['/verify/otp']); }
}
