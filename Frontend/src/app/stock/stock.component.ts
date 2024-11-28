import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { SharedService } from '../shared/shared.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-stock',
  templateUrl: './stock.component.html',
  styleUrls: ['./stock.component.css']
})
export class StockComponent {
  selectedFile: File | null = null;
  portfolioData: any;
  dataSource: any = [];

  constructor(
    private http: HttpClient,
    private router: Router,
    private shared: SharedService,
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
          'https://localhost:44394/api/Investment/AddInvestmentDetails?fileName=Stocks',
          formData
        )
        .subscribe((res) => {
          if (res.success == true) {
            this.toastr.success('File Uploaded successful!', 'Success');
          } else {
            this.toastr.error(
              'Issue in Uploading!!, please try again.',
              'Error'
            );
          }
          this.getInvestmentData();
        });
    }
  }

  getInvestmentData() {
    debugger;
    this.http
      .get<any[]>(
        'https://localhost:44394/api/Investment/GetInvestedDetails?Investmentname=Stocks'
      )
      .subscribe((res) => {
        console.log('Response data:', res);
        this.portfolioData = res; // Ensure that the response is an array
        console.log(this.portfolioData);
      });
  }

  goBack(): void {
    this.router.navigate(['/home']); // Adjust this to your desired previous route
  }
}
