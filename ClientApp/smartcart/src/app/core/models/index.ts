export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface OtpVerifyResponse {
  exists: boolean;
  userId?: number;
  name?: string;
  email?: string;
}

export interface UpsertUserResponse {
  userId: number;
  name: string;
  email: string;
}

export interface CartItem {
  uids: string[];
  productId: number;
  name: string;
  price: number;
  imageUrl: string | null;
  categoryName: string;
  quantity: number;
}

export interface CartToast {
  type: 'add' | 'remove';
  name: string;
}

export interface CartUser {
  userId: number;
  name: string;
  email: string;
  mobile: string;
}

export interface CartSession {
  sessionId: number;
  cartId: number;
  status: string;
}

export interface PaymentCard {
  maskedCardNumber: string;
  holderName: string;
  expiry: string;
}

export interface PaymentResult {
  success: boolean;
  invoiceNumber: string;
  amount: number;
}

// SignalR event payloads
export interface CartUpdatedEvent {
  type: 'item_added' | 'item_removed' | 'payment_card_tapped';
  uid: string;
  productId?: number;
  name?: string;
  price?: number;
  imageUrl?: string | null;
  categoryName?: string;
  action?: string;
  timestamp?: string;
}

export interface SessionUpdatedEvent {
  type: 'session_started' | 'checkout_initiated' | 'session_reset';
  sessionId?: number;
  status?: string;
  cartId?: number;
}

export interface PaymentUpdatedEvent {
  type: 'payment_processing' | 'payment_failed' | 'payment_success';
  sessionId: number;
  invoiceNumber?: string;
  amount?: number;
  message?: string;
}
