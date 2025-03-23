import { Injectable } from '@angular/core';


@Injectable({
  providedIn: 'root'
})
export class SharedService {
  private sharedData: any;

  constructor() { }

    // Function to calculate the holding period for each stock
    calculateHoldingPeriods(data: any[]): { stockName: string; holdingPeriod: number }[] {
      debugger
        const stockData: {
          [key: string]: { buyDates: string[]; sellDates: string[] };
        } = {};
    
        console.log(data);
        
        if(data && Array.isArray(data)){
          data.forEach((item) => {
            const { stockName, buyDate, sellDate } = item;
      
            // Convert dates from DD-MM-YYYY to YYYY-MM-DD
            const formattedBuyDate = this.convertDateFormat(buyDate);
            const formattedSellDate = this.convertDateFormat(sellDate);
      
            // Validate the converted dates
            if (
              this.isValidDate(formattedBuyDate) &&
              this.isValidDate(formattedSellDate)
            ) {
              if (!stockData[stockName]) {
                stockData[stockName] = { buyDates: [], sellDates: [] };
              }
              stockData[stockName].buyDates.push(formattedBuyDate);
              stockData[stockName].sellDates.push(formattedSellDate);
            } 
            else {
              console.error(
                `Invalid date for stock ${stockName}: BuyDate: ${buyDate}, SellDate: ${sellDate}`
              );
            }
          });
        }

    
        const holdingPeriods: { stockName: string; holdingPeriod: number }[] = [];

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
                holdingPeriods.push({ stockName, holdingPeriod });
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
            holdingPeriods.push({ stockName, holdingPeriod });
            console.log(`Holding period for ${stockName}: ${holdingPeriod} days`);
          }
        });
        return holdingPeriods;
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
    
      calculatePeriod(buyDate: Date, sellDate: Date): number {
        const timeDifference = sellDate.getTime() - buyDate.getTime();
        if (timeDifference > 0) {
          return Math.floor(timeDifference / (1000 * 3600 * 24)); // Days
        }
        return 0; // Return the difference in days
      }

      setData(data:any):void{
        debugger
        this.sharedData =data;
        sessionStorage.setItem("Data",JSON.stringify(this.sharedData))
        console.log(this.sharedData);
        
      }

      getData(){
        return this.sharedData
      }

}