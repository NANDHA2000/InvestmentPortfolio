import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { URL_LIST } from '../Config/url.config';


@Component({
  selector: 'app-stock',
  templateUrl: './stock.component.html',
  styleUrls: ['./stock.component.css']
})

export class StockComponent {
  selectedFile: File | null = null;
  portfolioData: any = { StockHoldings: [] };
  dataSource: any = [];


  constructor(
    private http: HttpClient,
    private router: Router,
    private toastr: ToastrService
  ) {}

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
        .post<any>(
          URL_LIST.ADD_STOCK_DETAILS +'/?fileName=Stocks',
          formData
        )
        .subscribe((res) => {
          if (res.success == true) {
            this.toastr.success('File Uploaded successful!', 'Success');
            this.getInvestmentData();
          } else {
            this.toastr.error(
              'Issue in Uploading!!, please try again.',
              'Error'
            );
          }
        });
    }
  }

  // getInvestmentData() {
  //   this.http
  //     .get<any[]>(
  //       URL_LIST.GET_STOCK_DETAILS +'/?Investmentname=Stocks'
  //     )
  //     .subscribe((res) => {
  //       this.portfolioData = res; // Ensure that the response is an array
  //       console.log(this.portfolioData);
  //     });
  // }

  getInvestmentData() {
    this.http
      .get<any>(
        URL_LIST.GET_STOCK_DETAILS + '/?investmentTypeId=1'
      )
      .subscribe((res) => {
        // Ensure correct mapping of nested StockHoldings
        this.portfolioData = {
          StockHoldings: res?.StockHoldings || []
        };
        console.log(this.portfolioData);
        
      });
  }
  


  goBack(): void {
    this.router.navigate(['/landingpage']); // Adjust this to your desired previous route
  }
}
