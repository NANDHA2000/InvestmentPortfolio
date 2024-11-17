import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  selectedFile: File | null = null;
  portfolioData: any[] = []; // Ensure it's initialized as an array


  constructor(private http: HttpClient) {}

  // Handle file selection event
  onFileSelect(event: any): void {
    this.selectedFile = event.target.files[0];
  }

  // Upload the file to the backend API
  // onUpload(): void {
  //   debugger;
  //   if (this.selectedFile) {
  //   const formData = new FormData();
  //   formData.append('file', this.selectedFile);
  //   this.http.post("https://localhost:44394/api/Excel/upload",formData).subscribe(res=>{
  //     this.portfolioData = res.data; 
  //     console.log("this.portfolioData",this.portfolioData);
  //   });
  // }

    
  // }

  onUpload(): void {
    if (this.selectedFile) {
      const formData = new FormData();
      formData.append('file', this.selectedFile);
  
      this.http.post<any[]>('https://localhost:44394/api/Excel/upload', formData).subscribe(
        res => {
          console.log("Response data:", res);
          this.portfolioData = res; // Ensure that the response is an array
        },
        error => {
          console.error("Error:", error);
        }
      );
    }
  }
  
}
