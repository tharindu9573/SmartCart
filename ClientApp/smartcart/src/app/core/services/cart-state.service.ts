import { Injectable, signal, computed } from '@angular/core';
import { CartItem, CartToast, CartUser, CartSession, PaymentCard, PaymentResult } from '../models';

@Injectable({ providedIn: 'root' })
export class CartStateService {
  // ── Auth / App ────────────────────────────────────────────────────────────
  readonly cartId = signal<number>(0);

  // ── User ──────────────────────────────────────────────────────────────────
  readonly user = signal<CartUser | null>(null);
  readonly mobileNumber = signal<string>('');

  // ── Session ───────────────────────────────────────────────────────────────
  readonly session = signal<CartSession | null>(null);

  // ── Cart Items ────────────────────────────────────────────────────────────
  readonly items = signal<CartItem[]>([]);
  readonly toast = signal<CartToast | null>(null);

  readonly totalItems = computed(() =>
    this.items().reduce((sum, i) => sum + i.quantity, 0)
  );

  readonly subtotal = computed(() =>
    this.items().reduce((sum, i) => sum + i.price * i.quantity, 0)
  );

  // ── Payment ───────────────────────────────────────────────────────────────
  readonly selectedPaymentMethod = signal<string>('');
  readonly paymentCard = signal<PaymentCard | null>(null);
  readonly paymentToken = signal<string>('');
  readonly paymentResult = signal<PaymentResult | null>(null);

  // ── Cart logic ────────────────────────────────────────────────────────────
  addItem(item: Omit<CartItem, 'quantity'>): void {
    this.items.update(current => {
      const existing = current.find(i => i.productId === item.productId);
      if (existing) {
        return current.map(i =>
          i.productId === item.productId
            ? { ...i, uids: [...i.uids, ...item.uids], quantity: i.quantity + item.uids.length }
            : i
        );
      }
      return [...current, { ...item, quantity: item.uids.length }];
    });
    this.showToast({ type: 'add', name: item.name });
  }

  removeItem(uid: string, name: string): void {
    this.items.update(current => {
      return current
        .map(i => {
          if (!i.uids.includes(uid)) return i;
          const newUids = i.uids.filter(u => u !== uid);
          return { ...i, uids: newUids, quantity: newUids.length };
        })
        .filter(i => i.quantity > 0);
    });
    this.showToast({ type: 'remove', name });
  }

  clearCart(): void {
    this.items.set([]);
    this.toast.set(null);
    this.session.set(null);
    this.user.set(null);
    this.mobileNumber.set('');
    this.paymentCard.set(null);
    this.paymentToken.set('');
    this.paymentResult.set(null);
    this.selectedPaymentMethod.set('');
  }

  private toastTimer: ReturnType<typeof setTimeout> | null = null;

  private showToast(toast: CartToast): void {
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toast.set(toast);
    this.toastTimer = setTimeout(() => this.toast.set(null), 3000);
  }
}
