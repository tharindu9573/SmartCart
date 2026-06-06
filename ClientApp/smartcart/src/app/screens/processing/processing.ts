import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { SignalRService } from '../../core/services/signalr.service';
import { ApiService } from '../../core/services/api.service';
import { CartStateService } from '../../core/services/cart-state.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-processing',
  imports: [CommonModule],
  templateUrl: './processing.html',
  styleUrl: './processing.css',
})
export class ProcessingComponent implements OnInit, OnDestroy {
  vatRate = environment.vatRate;
  private subs = new Subscription();

  constructor(
    private router: Router,
    private signalR: SignalRService,
    private api: ApiService,
    public state: CartStateService
  ) {}

  get card() { return this.state.paymentCard(); }
  get subtotal() { return this.state.subtotal(); }
  get total() { return this.subtotal + this.subtotal * this.vatRate; }

  ngOnInit(): void {
    const session = this.state.session();
    const token = this.state.paymentToken();
    if (!session || !token) { this.router.navigate(['/cart']); return; }

    // Subscribe to payment result events from SignalR
    this.subs.add(
      this.signalR.paymentUpdated$.subscribe(event => {
        if (event.type === 'payment_success') {
          this.state.paymentResult.set({
            success: true,
            invoiceNumber: event.invoiceNumber ?? '',
            amount: event.amount ?? this.total,
          });
          this.router.navigate(['/checkout/success']);
        } else if (event.type === 'payment_failed') {
          this.router.navigate(['/checkout/failed']);
        }
      })
    );

    // Trigger payment call
    this.processPayment(session.sessionId, token);
  }

  ngOnDestroy(): void { this.subs.unsubscribe(); }

  private async processPayment(sessionId: number, token: string): Promise<void> {
    try {
      const result = await this.api.processPayment(
        sessionId,
        this.state.selectedPaymentMethod(),
        token,
        this.total
      );
      if (result.success) {
        this.state.paymentResult.set(result);
        this.router.navigate(['/checkout/success']);
      } else {
        this.router.navigate(['/checkout/failed']);
      }
    } catch (err) {
      // 400 Bad Request (e.g. insufficient balance) or network error → failed screen
      console.error('Payment error:', ApiService.extractError(err));
      this.router.navigate(['/checkout/failed']);
    }
  }
}
