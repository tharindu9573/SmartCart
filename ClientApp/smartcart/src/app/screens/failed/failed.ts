import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CartStateService } from '../../core/services/cart-state.service';

@Component({
  selector: 'app-failed',
  templateUrl: './failed.html',
  styleUrl: './failed.css',
})
export class FailedComponent {
  constructor(private router: Router, private state: CartStateService) {}

  retry(): void { this.router.navigate(['/checkout/tap']); }

  chooseOther(): void { this.router.navigate(['/checkout/payment']); }
}
