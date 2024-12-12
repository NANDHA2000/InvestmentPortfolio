import { Component, NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { authGuard } from './shared/auth.guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { LandingPageComponent } from './landing-page/landing-page.component';
import { MutualFundComponent } from './mutual-fund/mutual-fund.component';
import { StockComponent } from './stock/stock.component';
import { FileVaultComponent } from './shared/file-vault/file-vault.component';

const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  {
    path: 'landingpage',
    component: LandingPageComponent,
    canActivate: [authGuard],
  },
  { path: 'mutualfund', component: MutualFundComponent,canActivate: [authGuard] },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard],
  },
  { path: 'stocks', component: StockComponent, canActivate: [authGuard] },
  { path: 'documentvault', component: FileVaultComponent, canActivate: [authGuard] },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
