import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardQuotationComponent } from './dashboard-quotation/dashboard-quotation.component';
import { CreateQuotationComponent } from './create-quotation/create-quotation.component';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardQuotationComponent },
  { path: 'create', component: CreateQuotationComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RateQuotationRoutingModule {}
