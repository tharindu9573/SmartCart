import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import {
  CartUpdatedEvent,
  SessionUpdatedEvent,
  PaymentUpdatedEvent,
} from '../models';

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private hub: signalR.HubConnection | null = null;

  readonly cartUpdated$ = new Subject<CartUpdatedEvent>();
  readonly sessionUpdated$ = new Subject<SessionUpdatedEvent>();
  readonly paymentUpdated$ = new Subject<PaymentUpdatedEvent>();

  constructor(private auth: AuthService) {}

  async connect(): Promise<void> {
    if (this.hub && this.hub.state === signalR.HubConnectionState.Connected) return;

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        accessTokenFactory: () => this.auth.ensureAuthenticated(),
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hub.on('CartUpdated', (json: string) => {
      try { this.cartUpdated$.next(JSON.parse(json)); } catch {}
    });

    this.hub.on('SessionUpdated', (json: string) => {
      try { this.sessionUpdated$.next(JSON.parse(json)); } catch {}
    });

    this.hub.on('PaymentUpdated', (json: string) => {
      try { this.paymentUpdated$.next(JSON.parse(json)); } catch {}
    });

    await this.hub.start();
  }

  async disconnect(): Promise<void> {
    if (this.hub) {
      await this.hub.stop();
      this.hub = null;
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}
