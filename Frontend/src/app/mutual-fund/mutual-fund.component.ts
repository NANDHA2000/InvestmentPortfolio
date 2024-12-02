import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { MF_URL_LIST } from '../Config/mutualfund-url.config';
import { Router } from '@angular/router';

@Component({
  selector: 'app-mutual-fund',
  templateUrl: './mutual-fund.component.html',
  styleUrls: ['./mutual-fund.component.css']
})
export class MutualFundComponent implements OnInit {
  selectedFile: File | null = null;
  mutualFundData: any;

  constructor(private http: HttpClient,private router: Router) {}

  
  ngOnInit(): void {
    this.getInvestmentData();
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
          MF_URL_LIST.ADD_MF_DETAILS +'/?fileName=MutualFund',
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
    .get<any[]>(MF_URL_LIST.GET_MF_DETAILS +'/?Investmentname=MutualFund')
    .subscribe(
      (res) => {
        this.mutualFundData = res; 
      }
    );}
  
    goBack(): void {
      this.router.navigate(['/landingpage']); // Adjust this to your desired previous route
    }
  }
