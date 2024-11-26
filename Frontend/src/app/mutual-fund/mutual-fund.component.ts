import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-mutual-fund',
  templateUrl: './mutual-fund.component.html',
  styleUrls: ['./mutual-fund.component.css']
})
export class MutualFundComponent implements OnInit {
  selectedFile: File | null = null;
  mutualFundData: any = [];

  constructor(private http: HttpClient) {}

  
  ngOnInit(): void {

  }

  onFileSelect(event: any): void {
    this.selectedFile = event.target.files[0];
  }


  onUpload() {
    if (this.selectedFile) {
      const formData = new FormData();
      formData.append('file', this.selectedFile);

      this.http
        .post<any[]>(
          'https://localhost:44394/upload',
          formData
        )
        .subscribe(
          (res) => {
            this.mutualFundData =res;
    });
  }
}

getInvestmentData() {
  this.http
    .get<any[]>('https://localhost:44394/api/Excel/InvestmentData')
    .subscribe(
      (res) => {
        console.log('Response data:', res);
        this.mutualFundData = res; 
      }
    );}
  
  
  }
