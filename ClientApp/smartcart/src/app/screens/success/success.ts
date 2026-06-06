import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CartStateService } from '../../core/services/cart-state.service';
import { ApiService } from '../../core/services/api.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-success',
  imports: [CommonModule],
  templateUrl: './success.html',
  styleUrl: './success.css',
})
export class SuccessComponent implements OnInit, OnDestroy {
  readonly countdown = signal(10);
  private timerRef: ReturnType<typeof setInterval> | null = null;

  constructor(private router: Router, public state: CartStateService, private api: ApiService) {}

  get result() { return this.state.paymentResult(); }
  get user() { return this.state.user(); }
  get card() { return this.state.paymentCard(); }

  ngOnInit(): void {
    this.timerRef = setInterval(() => {
      this.countdown.update(n => n - 1);
      if (this.countdown() <= 0) this.startAgain();
    }, 1000);
  }

  ngOnDestroy(): void {
    if (this.timerRef) clearInterval(this.timerRef);
  }

  startAgain(): void {
    if (this.timerRef) { clearInterval(this.timerRef); this.timerRef = null; }
    this.api.resetSession(environment.cartId).catch(() => {});
    this.state.clearCart();
    this.router.navigate(['/']);
  }
}
