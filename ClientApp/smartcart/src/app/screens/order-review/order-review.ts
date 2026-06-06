import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CartStateService } from '../../core/services/cart-state.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-order-review',
  imports: [CommonModule],
  templateUrl: './order-review.html',
  styleUrl: './order-review.css',
})
export class OrderReviewComponent implements OnInit {
  loading = false;
  vatRate = environment.vatRate;

  constructor(
    private router: Router,
    private api: ApiService,
    public state: CartStateService
  ) { }

  get items() { return this.state.items(); }
  get subtotal() { return this.state.subtotal(); }
  get vat() { return this.subtotal * this.vatRate; }
  get total() { return this.subtotal + this.vat; }

  ngOnInit(): void {
    if (!this.state.session() || this.items.length === 0) {
      this.router.navigate(['/cart']);
    }
  }

  async confirmAndPay(): Promise<void> {
    const session = this.state.session();
    if (!session) return;
    this.loading = true;
    try {
      const uids = this.items.flatMap(i => i.uids);
      await this.api.confirmCart(session.sessionId, uids);
      this.router.navigate(['/checkout/payment']);
    } catch {
      this.router.navigate(['/checkout/payment']);
    } finally {
      this.loading = false;
    }
  }

  back(): void { this.router.navigate(['/cart']); }
}
