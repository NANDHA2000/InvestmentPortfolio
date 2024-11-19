import { Component, NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InvestmentPortfolioComponent } from './investment-portfolio/investment-portfolio.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { AuthService } from './shared/auth.service';
import { AuthGuard } from './shared/auth.guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { LandingPageComponent } from './landing-page/landing-page.component';

const routes: Routes = [
  { path: '', component:HomeComponent  },
  { path: 'home', component: HomeComponent},
  { path: 'login', component: LoginComponent },
  { path: 'landingpage',component:LandingPageComponent},
  { path: 'dashboard', component: DashboardComponent },
  { path: 'portfolio', component: InvestmentPortfolioComponent}

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
