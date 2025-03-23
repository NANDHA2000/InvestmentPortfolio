import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { URL_LIST } from '../Config/url.config';



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
      .get<any[]>(URL_LIST.NAVBAR)
      .subscribe((res) => {
        this.navLinks = res;
        console.log(this.navLinks);
        
      });
  }

  logout() {
    localStorage.removeItem('userToken');
    this.router.navigate(['/login']);
  }
}
