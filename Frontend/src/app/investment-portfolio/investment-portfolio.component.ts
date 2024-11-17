import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-investment-portfolio',
  templateUrl: './investment-portfolio.component.html',
  styleUrls: ['./investment-portfolio.component.css'],
})
export class InvestmentPortfolioComponent {
  selectedFile: File | null = null;
  portfolioData: any[] = [];
  unrealisedStocks: any[] =[];
  isFileUploaded = false;


  constructor(private http: HttpClient) {}

  ngOnInit() {
   // this.getInvestmentData();
  }

  // Handle file selection event
  onFileSelect(event: any): void {
    this.selectedFile = event.target.files[0];
  }

  onUpload() {
    if (this.selectedFile) {
      const formData = new FormData();
      formData.append('file', this.selectedFile);

      this.http
        .post<any[]>(
          'https://localhost:44394/api/Excel/ReadStockData',
          formData
        )
        .subscribe(
          (res) => {
            console.log('Response data:', res);
            //this.portfolioData = res; // Ensure that the response is an array
            this.unrealisedStocks = res.filter(
              (item) => item.Stockname === 'Stock name'
            );
            this.portfolioData = res;
            this.isFileUploaded = true;
            console.log('Unrealised Stocks:', this.unrealisedStocks);
          },
          (error) => {
            console.error('Error:', error);
          }
        );
    }
  }

  // getInvestmentData() {
  //     this.http.get<any[]>('https://localhost:44394/api/Excel/InvestmentData').subscribe(
  //       res => {
  //         console.log("Response data:", res);
  //         this.portfolioData = res; // Ensure that the response is an array
  //       },
  //       error => {
  //         console.error("Error:", error);
  //       }
  //     );
  // }
}
