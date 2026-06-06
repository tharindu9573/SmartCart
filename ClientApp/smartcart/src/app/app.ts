import { Component, OnInit } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { SignalRService } from './core/services/signalr.service';
import { CartStateService } from './core/services/cart-state.service';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: '<router-outlet />',
  styles: [':host { display: block; width: 100%; height: 100%; }'],
})
export class App implements OnInit {
  constructor(
    private auth: AuthService,
    private signalR: SignalRService,
    private state: CartStateService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    this.state.cartId.set(environment.cartId);
    try {
      await this.auth.ensureAuthenticated();
      await this.signalR.connect();
    } catch (err) {
      console.error('Startup auth/SignalR failed:', err);
    }

    this.signalR.sessionUpdated$.subscribe(event => {
      if (event.type === 'session_reset') {
        const url = this.router.url;
        const isVerifyPage = url.startsWith('/verify/');
        if (!isVerifyPage) {
          this.state.clearCart();
          this.router.navigate(['/']);
        }
      }
    });
  }
}
