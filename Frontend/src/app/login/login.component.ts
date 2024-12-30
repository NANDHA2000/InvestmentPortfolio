import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { URL_LIST } from '../Config/url.config';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  loginForm: FormGroup;
  registerForm: FormGroup;
  activeTab: 'login' | 'register' = 'login';
  loginData: any;
  email: any;
  password: any;
  registerData: any;
  Name: any;

  constructor(private fb: FormBuilder,private http: HttpClient,private router: Router,private toastr: ToastrService){
  
    // Initialize Login Form
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });

    // Initialize Register Form
    this.registerForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });

  }

  setActiveTab(tab: 'login' | 'register'): void {
    this.activeTab = tab;
  }
  // Login form submission
  onLogin(): void {
    if (this.loginForm.valid) {
       this.loginData = this.loginForm.value;
       this.email = this.loginData.email
       this.password =this.loginData.password
      console.log('Login Data:', this.email,this.password);

      const loginPayload = {
        email: this.email, // Map email to username for backend compatibility
        password: this.password
      };
      // Perform login API call here
      this.http.post<any>(URL_LIST.LOGIN,loginPayload).subscribe(
        (res) => {
          console.log(res);
            if(res.success == true){
              console.log("Login successful",res);
              localStorage.setItem('userToken', loginPayload.email);
              this.toastr.success('Login successful!', 'Success');
              this.router.navigate(['/landingpage']);
            }
            else {
              console.log("Invalid credentials",res);
              this.toastr.error('Invalid credentials, please try again.', 'Error');
            }
        },
        error => {
          console.error("Error:", error);
        }
      );
    } else {
      console.log('Login form is invalid');
    }
  }

  // Register form submission
  onRegister(): void {
    if (this.registerForm.valid) {
      if (this.registerForm.value.password !== this.registerForm.value.confirmPassword) {
        console.error('Passwords do not match');
        return;
      }
      this.registerData = this.registerForm.value;
      this.Name = this.registerData.name
      this.email = this.registerData.email
      this.password =this.registerData.password
      console.log('Register Data:', this.registerData);

      const RegisterPayload = {
        Name : this.Name,
        email: this.email, 
        password: this.password
      };
      // Perform register API call here
      this.http.post<any>(URL_LIST.REGISTER,RegisterPayload).subscribe(
        (res) => {
          console.log(res);
          
            if(res.success == true){
              console.log("Login successful",res);
              this.toastr.success('Register successful!', 'Success');
              this.setActiveTab("login")
            }
            else {
              console.log("Invalid credentials",res);
              this.toastr.error('Invalid credentials, please try again.', 'Error');
            }
        },
        error => {
          console.error("Error:", error);
        }
      );
    } else {
      console.log('Register form is invalid');
    }
  }
}

