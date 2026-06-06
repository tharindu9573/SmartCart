import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CartStateService } from '../../core/services/cart-state.service';
import { environment } from '../../../environments/environment';

interface PayOption {
  id: string;
  icon: string;
  title: string;
  sub: string;
}

@Component({
  selector: 'app-payment-method',
  imports: [CommonModule],
  templateUrl: './payment-method.html',
  styleUrl: './payment-method.css',
})
export class PaymentMethodComponent implements OnInit {
  vatRate = environment.vatRate;
  selected = 'contactless';

  options: PayOption[] = [
    { id: 'contactless', icon: '&#128179;', title: 'Contactless Card', sub: 'Tap your card to the reader' },
    { id: 'paypal',      icon: '&#127185;', title: 'PayPal',           sub: 'Scan QR with your app' },
    { id: 'mobile',      icon: '&#128241;', title: 'Mobile Pay',       sub: 'Apple Pay / Google Pay' },
  ];

  constructor(private router: Router, public state: CartStateService) {}

  get subtotal() { return this.state.subtotal(); }
  get total() { return this.subtotal + this.subtotal * this.vatRate; }

  ngOnInit(): void {
    if (!this.state.session()) { this.router.navigate(['/cart']); }
  }

  select(id: string): void { this.selected = id; }

  proceed(): void {
    this.state.selectedPaymentMethod.set(this.selected);
    this.router.navigate(['/checkout/tap']);
  }

  back(): void { this.router.navigate(['/checkout/review']); }
}
