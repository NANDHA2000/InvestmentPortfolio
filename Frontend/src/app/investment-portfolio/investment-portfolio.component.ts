import { Component, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { SharedService } from '../shared/shared.service';
interface DataRow {
  [key: string]: any; // To handle dynamic columns in the data
}

@Component({
  selector: 'app-investment-portfolio',
  templateUrl: './investment-portfolio.component.html',
  styleUrls: ['./investment-portfolio.component.css']
})
export class InvestmentPortfolioComponent {
  selectedFile: File | null = null;
  portfolioData: any[] = [];
  unrealisedStocks: any[] = [];
  RealisedPL: any[] = [];
  isFileUploaded = false;
  totalProfit: number = 0;
  totalLoss: number = 0;
  displayedColumns: string[] = [
    'Scheme Name', 'AMC', 'Category', 'Sub-category', 'Folio No.',
    'Units', 'Invested Value', 'Current Value', 'Returns', 'XIRR'
  ];
  dataSource :any = [];
  rawData: any[] = [];  // Your raw JSON data
  displayedData: DataRow[] = [];  // The processed and filtered data
  

  constructor(private http: HttpClient, private router: Router,private shared:SharedService) {}

  ngOnInit() {
    this.getInvestmentData();
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
            this.dataSource =res;
            console.log(this.dataSource);
            this.processData();
    });
  }
}

processData() {
  // Filter and transform the raw data to remove empty rows and keep relevant data
  this.displayedData = this.rawData.filter(row => {
    // Check if at least one column contains a value (i.e., non-empty object or string)
    return Object.values(row).some(value => value && Object.keys(value).length > 0);
  });
}

  // onUpload() {
  //   if (this.selectedFile) {
  //     const formData = new FormData();
  //     formData.append('file', this.selectedFile);

  //     this.http
  //       .post<any[]>(
  //         'https://localhost:44394/api/Excel/ReadStockData',
  //         formData
  //       )
  //       .subscribe(
  //         (res) => {
  //           console.log('Response data:', res);
  //           //this.portfolioData = res; // Ensure that the response is an array
  //           this.unrealisedStocks = res.filter(
  //             (item) => item.Stockname === 'Stock name'
  //           );
  //           this.portfolioData = res;
  //           this.isFileUploaded = true;
  //           const values = res.map((item) => item.RealisedPL);

  //           // Iterate through the data, filtering out non-numeric values
  //           values.forEach((item) => {
  //             const numValue = parseFloat(item);

  //             // Check if the value is a number and not the string 'Unrealised P&L'
  //             if (!isNaN(numValue)) {
  //               if (numValue >= 0) {
  //                 this.totalProfit += numValue;
  //               } else {
  //                 this.totalLoss += numValue;
  //               }
  //             }
  //           });
  //         },
  //         (error) => {
  //           console.error('Error:', error);
  //         }
  //       );
  //   }
  // }

  getInvestmentData() {
    debugger
    this.http
      .get<any[]>('https://localhost:44394/api/Excel/InvestmentData')
      .subscribe(
        (res) => {
          console.log('Response data:', res);
          this.portfolioData = res; // Ensure that the response is an array
          const values = res.map((item) => item.realisedPL);
          debugger
          this.shared.setData(res);

         // this.shared.calculateHoldingPeriods(res);

          // Iterate through the data, filtering out non-numeric values
          values.forEach((item) => {
            const numValue = parseFloat(item);

            // Check if the value is a number and not the string 'Unrealised P&L'
            if (!isNaN(numValue)) {
              if (numValue >= 0) {
                this.totalProfit += numValue;
              } else {
                this.totalLoss += numValue;
              }
            }
          });
        },
        (error) => {
          console.error('Error:', error);
        }
      );
  }

  goBack(): void {
    this.router.navigate(['/home']); // Adjust this to your desired previous route
  }
}
