import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./screens/welcome/welcome').then(m => m.WelcomeComponent),
  },
  {
    path: 'verify/mobile',
    loadComponent: () =>
      import('./screens/mobile-entry/mobile-entry').then(m => m.MobileEntryComponent),
  },
  {
    path: 'verify/otp',
    loadComponent: () =>
      import('./screens/otp-verify/otp-verify').then(m => m.OtpVerifyComponent),
  },
  {
    path: 'verify/confirm',
    loadComponent: () =>
      import('./screens/user-confirm/user-confirm').then(m => m.UserConfirmComponent),
  },
  {
    path: 'verify/setup',
    loadComponent: () =>
      import('./screens/user-setup/user-setup').then(m => m.UserSetupComponent),
  },
  {
    path: 'cart',
    loadComponent: () =>
      import('./screens/scanning/scanning').then(m => m.ScanningComponent),
  },
  {
    path: 'checkout/review',
    loadComponent: () =>
      import('./screens/order-review/order-review').then(m => m.OrderReviewComponent),
  },
  {
    path: 'checkout/payment',
    loadComponent: () =>
      import('./screens/payment-method/payment-method').then(m => m.PaymentMethodComponent),
  },
  {
    path: 'checkout/tap',
    loadComponent: () =>
      import('./screens/tap-card/tap-card').then(m => m.TapCardComponent),
  },
  {
    path: 'checkout/processing',
    loadComponent: () =>
      import('./screens/processing/processing').then(m => m.ProcessingComponent),
  },
  {
    path: 'checkout/success',
    loadComponent: () =>
      import('./screens/success/success').then(m => m.SuccessComponent),
  },
  {
    path: 'checkout/failed',
    loadComponent: () =>
      import('./screens/failed/failed').then(m => m.FailedComponent),
  },
  { path: '**', redirectTo: '' },
];
