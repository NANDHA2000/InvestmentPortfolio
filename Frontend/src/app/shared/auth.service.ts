import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor() { }

  isLoggedIn(): boolean {
    // Replace with actual authentication logic
    return !!localStorage.getItem('email'); // Example: Check if an email exists
  }
}
