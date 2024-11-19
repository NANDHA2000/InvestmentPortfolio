import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';

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
      .get<any[]>('https://localhost:44394/api/Login/GetNavBar')
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
