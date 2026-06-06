import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CartStateService } from '../../core/services/cart-state.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-user-setup',
  imports: [CommonModule, FormsModule],
  templateUrl: './user-setup.html',
  styleUrl: './user-setup.css',
})
export class UserSetupComponent {
  name = '';
  email = '';
  loading = false;
  error = '';

  constructor(
    private router: Router,
    private api: ApiService,
    private state: CartStateService
  ) {}

  get mobile(): string { return this.state.mobileNumber(); }

  async submit(): Promise<void> {
    if (!this.name.trim() || !this.email.trim()) {
      this.error = 'Please fill in all fields.';
      return;
    }
    this.loading = true;
    this.error = '';
    try {
      const user = await this.api.upsertUser(this.mobile, this.name.trim(), this.email.trim());
      this.state.user.set({ userId: user.userId, name: user.name, email: user.email, mobile: this.mobile });
      const session = await this.api.startSession(environment.cartId, user.userId);
      this.state.session.set({ sessionId: session.sessionId, cartId: environment.cartId, status: session.status });
      this.router.navigate(['/cart']);
    } catch (err) {
      this.error = ApiService.extractError(err, 'Could not create account. Please try again.');
    } finally {
      this.loading = false;
    }
  }

  back(): void { this.router.navigate(['/verify/otp']); }
}
