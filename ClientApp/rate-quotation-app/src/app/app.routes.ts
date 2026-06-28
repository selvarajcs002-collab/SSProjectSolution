import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'rate-quotation', pathMatch: 'full' },
  { path: 'rate-quotation', loadChildren: () => import('./rate-quotation/rate-quotation.module').then(m => m.RateQuotationModule) }
];
