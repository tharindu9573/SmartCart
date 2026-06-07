import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { SignalRService } from '../../core/services/signalr.service';
import { ApiService } from '../../core/services/api.service';
import { CartStateService } from '../../core/services/cart-state.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-scanning',
  imports: [CommonModule],
  templateUrl: './scanning.html',
  styleUrl: './scanning.css',
})
export class ScanningComponent implements OnInit, OnDestroy {
  private subs = new Subscription();

  cartLabel = `CART-${String(environment.cartId).padStart(2, '0')}`;

  constructor(
    private router: Router,
    public state: CartStateService,
    private signalR: SignalRService,
    private api: ApiService
  ) {}

  get user() { return this.state.user(); }
  get items() { return this.state.items(); }
  get toast() { return this.state.toast(); }
  get totalItems() { return this.state.totalItems(); }
  get subtotal() { return this.state.subtotal(); }

  ngOnInit(): void {
    if (!this.state.session()) {
      this.restoreSession();
      return;
    }
    this.subscribeToSignalR();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  private async restoreSession(): Promise<void> {
    try {
      const data = await this.api.getActiveSession(environment.cartId);
      this.state.session.set({ sessionId: data.sessionId, cartId: environment.cartId, status: data.status });
      this.state.items.set(
        data.items.map((i: any) => ({
          uids: i.uids ?? Array.from({ length: i.quantity }, (_: any, idx: number) => `${i.productId}-restored-${idx}`),
          productId: i.productId,
          name: i.name,
          price: i.price,
          imageUrl: i.imageUrl ?? null,
          categoryName: i.categoryName ?? '',
          quantity: i.quantity,
        }))
      );
      this.subscribeToSignalR();
    } catch {
      this.router.navigate(['/']);
    }
  }

  private subscribeToSignalR(): void {
    this.subs.add(
      this.signalR.cartUpdated$.subscribe(event => {
        if (event.type === 'item_added' && event.productId && event.name !== undefined && event.price !== undefined) {
          this.state.addItem({
            uids: [event.uid],
            productId: event.productId,
            name: event.name!,
            price: event.price!,
            imageUrl: event.imageUrl ?? null,
            categoryName: event.categoryName ?? '',
          });
        } else if (event.type === 'item_removed') {
          // Match by uid (live scan) or productId fallback (restored items)
          const existing = this.items.find(i => i.uids.includes(event.uid))
            ?? (event.productId ? this.items.find(i => i.productId === event.productId) : undefined);
          if (existing) {
            const uidToRemove = existing.uids.includes(event.uid) ? event.uid : existing.uids[0];
            this.state.removeItem(uidToRemove, existing.name);
          }
        }
      })
    );

  }

  async checkout(): Promise<void> {
    if (this.items.length === 0) return;
    const session = this.state.session();
    if (!session) return;
    this.router.navigate(['/checkout/review']);
  }
}
