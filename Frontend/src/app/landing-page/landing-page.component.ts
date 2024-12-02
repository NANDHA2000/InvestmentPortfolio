import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { LANDING_PAGE_URL_LIST } from '../Config/landing-page.config';


@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.css'],
})
export class LandingPageComponent {
  navLinks: any;

  constructor(private http: HttpClient,private router: Router) {}

  ngOnInit(): void {
    this.getNavData();
  }

  getNavData() {
    this.http
      .get<any[]>(LANDING_PAGE_URL_LIST.NAVBAR)
      .subscribe((res) => {
        console.log('Response data:', res);
        this.navLinks = res;
      });
  }

  logout(): void {
    // Clear token from localStorage
    localStorage.removeItem('token');
    console.log('User logged out successfully.');

    // Redirect to Login Page
    this.router.navigate(['/login']);
  }
}
