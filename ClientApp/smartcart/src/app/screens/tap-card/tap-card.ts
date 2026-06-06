import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { SignalRService } from '../../core/services/signalr.service';
import { ApiService } from '../../core/services/api.service';
import { CartStateService } from '../../core/services/cart-state.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-tap-card',
  imports: [CommonModule],
  templateUrl: './tap-card.html',
  styleUrl: './tap-card.css',
})
export class TapCardComponent implements OnInit, OnDestroy {
  vatRate = environment.vatRate;
  private subs = new Subscription();
  resolving = false;
  error = '';

  constructor(
    private router: Router,
    private signalR: SignalRService,
    private api: ApiService,
    public state: CartStateService
  ) {}

  get subtotal() { return this.state.subtotal(); }
  get total() { return this.subtotal + this.subtotal * this.vatRate; }

  ngOnInit(): void {
    if (!this.state.session()) { this.router.navigate(['/cart']); return; }

    // Receive payment UID pushed by backend after RFID payment scan
    this.subs.add(
      this.signalR.cartUpdated$.subscribe(async event => {
        if (event.type === 'payment_card_tapped' && event.uid) {
          await this.handlePaymentUid(event.uid);
        }
      })
    );
  }

  ngOnDestroy(): void { this.subs.unsubscribe(); }

  private async handlePaymentUid(uid: string): Promise<void> {
    this.error = '';
    this.resolving = true;
    try {
      // Step 1: resolve the payment token → get masked card details
      const card = await this.api.resolveToken(uid);
      this.state.paymentCard.set(card);
      this.state.paymentToken.set(uid);
      // Step 2: navigate to processing — that screen calls /payment/process
      this.router.navigate(['/checkout/processing']);
    } catch (err) {
      this.resolving = false;
      this.error = ApiService.extractError(err, 'Card not recognised. Please try a different card.');
    }
  }

  back(): void { this.router.navigate(['/checkout/payment']); }
}
