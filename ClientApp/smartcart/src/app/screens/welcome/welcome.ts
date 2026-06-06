import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CartStateService } from '../../core/services/cart-state.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.html',
  styleUrl: './welcome.css',
})
export class WelcomeComponent {
  cartLabel = `CART-${String(environment.cartId).padStart(2, '0')}`;

  constructor(private router: Router, private state: CartStateService) {}

  start(): void {
    this.state.clearCart();
    this.router.navigate(['/verify/mobile']);
  }
}
