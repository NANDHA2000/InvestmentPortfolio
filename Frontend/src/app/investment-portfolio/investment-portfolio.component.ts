import { Component, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-investment-portfolio',
  templateUrl: './investment-portfolio.component.html',
  styleUrls: ['./investment-portfolio.component.css'],
})
export class InvestmentPortfolioComponent {
  selectedFile: File | null = null;
  portfolioData: any[] = [];
  unrealisedStocks: any[] = [];
  RealisedPL: any[] = [];
  isFileUploaded = false;
  totalProfit: number = 0;
  totalLoss: number = 0;
  

  constructor(private http: HttpClient, private router: Router) {}

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
            console.log('Response data:', res);
            //this.portfolioData = res; // Ensure that the response is an array
            this.unrealisedStocks = res.filter(
              (item) => item.Stockname === 'Stock name'
            );
            this.portfolioData = res;
            this.isFileUploaded = true;
            const values = res.map((item) => item.RealisedPL);

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
  }

  getInvestmentData() {
    this.http
      .get<any[]>('https://localhost:44394/api/Excel/InvestmentData')
      .subscribe(
        (res) => {
          console.log('Response data:', res);
          this.portfolioData = res; // Ensure that the response is an array
          const values = res.map((item) => item.realisedPL);

          this.calculateHoldingPeriods(res);

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

  // Function to calculate the holding period for each stock
  calculateHoldingPeriods(data: any[]) {
    const stockData: {
      [key: string]: { buyDates: string[]; sellDates: string[] };
    } = {};

    data.forEach((item) => {
      const { stockname, buydate, selldate } = item;

      // Convert dates from DD-MM-YYYY to YYYY-MM-DD
      const formattedBuyDate = this.convertDateFormat(buydate);
      const formattedSellDate = this.convertDateFormat(selldate);

      // Validate the converted dates
      if (
        this.isValidDate(formattedBuyDate) &&
        this.isValidDate(formattedSellDate)
      ) {
        if (!stockData[stockname]) {
          stockData[stockname] = { buyDates: [], sellDates: [] };
        }
        stockData[stockname].buyDates.push(formattedBuyDate);
        stockData[stockname].sellDates.push(formattedSellDate);
      } 
      else {
        // console.error(
        //   `Invalid date for stock ${stockname}: BuyDate: ${buydate}, SellDate: ${selldate}`
        // );
      }
    });

    // Now calculate holding periods for each stock
    Object.keys(stockData).forEach((stockName) => {
      const stock = stockData[stockName];

      const sortedBuyDates = stock.buyDates.sort(
        (a, b) => new Date(a).getTime() - new Date(b).getTime()
      );
      const sortedSellDates = stock.sellDates.sort(
        (a, b) => new Date(a).getTime() - new Date(b).getTime()
      );

      // Process each sequence
      let startNewSequence = true; // Flag to start a new sequence
      let firstBuyDate: Date | null = null;
      let lastSellDate: Date | null = null;

      sortedBuyDates.forEach((buyDate, index) => {
        const sellDate = sortedSellDates[index];

        // If this buy date comes after the last sell date, start a new sequence
        if (
          firstBuyDate &&
          new Date(buyDate).getTime() > new Date(lastSellDate!).getTime()
        ) {
          // Calculate the holding period for the previous sequence
          if (firstBuyDate && lastSellDate) {
            const holdingPeriod = this.calculatePeriod(
              firstBuyDate,
              lastSellDate
            );
            console.log(
              `Holding period for ${stockName}: ${holdingPeriod} days`
            );
          }
          // Reset for the new sequence
          firstBuyDate = new Date(buyDate);
          lastSellDate = new Date(sellDate);
        } else if (startNewSequence) {
          // If it's the first sequence, initialize the firstBuyDate and lastSellDate
          firstBuyDate = new Date(buyDate);
          lastSellDate = new Date(sellDate);
          startNewSequence = false;
        } else {
          // Continue with the current sequence
          lastSellDate = new Date(sellDate);
        }
      });

      // After looping through all buys and sells, make sure the last sequence is processed
      if (firstBuyDate && lastSellDate) {
        const holdingPeriod = this.calculatePeriod(firstBuyDate, lastSellDate);
        console.log(`Holding period for ${stockName}: ${holdingPeriod} days`);
      }
    });
  }

  // Helper function to convert DD-MM-YYYY format to YYYY-MM-DD
  convertDateFormat(date: string): string {
    const parts = date.split('-');
    // Convert to YYYY-MM-DD
    return `${parts[2]}-${parts[1]}-${parts[0]}`;
  }

  // Helper function to check if a date is valid
  isValidDate(date: string): boolean {
    const parsedDate = new Date(date);
    return !isNaN(parsedDate.getTime()); // Check if the date is valid
  }

  calculatePeriod(buyDate: Date, sellDate: Date) {
    const timeDifference = sellDate.getTime() - buyDate.getTime();
    const diffInDays = timeDifference / (1000 * 3600 * 24); // Convert milliseconds to days
    return Math.floor(diffInDays); // Return the difference in days
  }

  goBack(): void {
    this.router.navigate(['/home']); // Adjust this to your desired previous route
  }
}
