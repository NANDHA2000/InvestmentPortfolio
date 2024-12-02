import { HttpClient } from '@angular/common/http';
import { Component, Input } from '@angular/core';
import { SharedService } from '../shared/shared.service';
import { STOCK_URL_LIST } from '../Config/stock-url.config';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent {
  chartDataRealised: any;
  chartDataUnRealised: any;
  chartOptionsRealised: any;
  chartOptionsUnRealised: any;
  totalProfit: number=0;
  totalLoss: number=0;
  portfolioData: any = {
    holdings: []
};
  basicData: any;
  basicOptions: any;
  GetData: any;

  constructor(private http: HttpClient,private shared:SharedService) {}


  ngOnInit():void{
    this.getInvestmentData();
    this.getbarChart();
    const dataString = sessionStorage.getItem("Data")
    const data = dataString ? JSON.parse(dataString) : null;

    
    
  }

// getInvestmentData() {
//   this.http.get<any>(STOCK_URL_LIST.GET_STOCK_DETAILS + '/?Investmentname=Stocks').subscribe(
//       res => {
//           if (res) {
//               this.portfolioData = { holdings: Array.isArray(res) ? res : res.holdings };

//               // Reset totals
//               this.totalProfit = 0;
//               this.totalLoss = 0;

//               // Loop through holdings and calculate profit/loss
//               let stopProcessing = false;
//               for (let item of this.portfolioData.holdings) {
//                   if (item.stockName === 'Unrealised trades') {
//                       stopProcessing = true; // Stop further processing when 'Un realised' is encountered
//                       break;
//                   }

//                   const realisedPL = parseFloat(item.realisedPL);
//                   if (!isNaN(realisedPL)) {
//                       if (realisedPL >= 0) {
//                           this.totalProfit += realisedPL;
//                       } else {
//                           this.totalLoss += realisedPL;
//                       }
//                   }
//               }

//               // Calculate overall P&L and update chart
//               const overallPL = this.totalProfit - Math.abs(this.totalLoss);

//               this.getpieChart(this.totalProfit, this.totalLoss, overallPL);

//           } else {
//               console.error('Empty or invalid response:', res);
//           }
//       },
//       error => {
//           console.error('Error fetching investment data:', error.status, error.message);
//       }
//   );
// }

getInvestmentData() {
  this.http.get<any>(STOCK_URL_LIST.GET_STOCK_DETAILS + '/?Investmentname=Stocks').subscribe(
      res => {
          if (res) {
              this.portfolioData = { holdings: Array.isArray(res) ? res : res.holdings };

              console.log(this.portfolioData);
              

              // Reset totals for both realised and unrealised profits/losses
              let realisedProfit = 0;
              let realisedLoss = 0;
              let unrealisedProfit = 0;
              let unrealisedLoss = 0;

              let isUnrealisedStarted = false; // Flag to track when "Unrealised trades" starts

              // Loop through holdings and calculate profit/loss before and after 'Unrealised trades'
              for (let item of this.portfolioData.holdings) {
                  // Once we encounter "Unrealised trades", start processing unrealised profit/loss
                  if (item.stockName === 'Unrealised trades') {
                      isUnrealisedStarted = true;
                      continue; // Skip this entry and move to the next for unrealised
                  }

                  const realisedPL = parseFloat(item.realisedPL);

                  if (!isNaN(realisedPL)) {
                      if (isUnrealisedStarted) {
                          // Calculate unrealised profit/loss
                          if (realisedPL >= 0) {
                              unrealisedProfit += realisedPL;
                          } else {
                              unrealisedLoss += realisedPL;
                          }
                      } else {
                          // Calculate realised profit/loss before "Unrealised trades"
                          if (realisedPL >= 0) {
                              realisedProfit += realisedPL;
                          } else {
                              realisedLoss += realisedPL;
                          }
                      }
                  }
              }

              // Calculate overall P&L for both realised and unrealised
              const totalRealisedPL = realisedProfit - Math.abs(realisedLoss);
              const totalUnrealisedPL = unrealisedProfit - Math.abs(unrealisedLoss);
              const overallPL = totalRealisedPL + totalUnrealisedPL;

              // Update chart with separate realised and unrealised profit/loss
              this.getpieChart('Realised',realisedProfit, realisedLoss, totalRealisedPL);
              this.getpieChart('UnRealised',unrealisedProfit, unrealisedLoss, totalUnrealisedPL)

              // Optionally, you can log or display the individual profits/losses
              console.log('Realised Profit:', realisedProfit);
              console.log('Realised Loss:', realisedLoss);
              console.log('Unrealised Profit:', unrealisedProfit);
              console.log('Unrealised Loss:', unrealisedLoss);
              console.log('Overall Profit/Loss:', overallPL);

          } else {
              console.error('Empty or invalid response:', res);
          }
      },
      error => {
          console.error('Error fetching investment data:', error.status, error.message);
      }
  );
}





getpieChart(type:string,profit:number,loss:number,overallpl:number){
  if(type == 'Realised'){
    this.chartDataRealised = {
      labels: ['Profit', 'Loss', 'OverallPL'],
      datasets: [
        {
          data: [profit,loss,overallpl],
          backgroundColor: ['green', 'red','yellow'],
          hoverBackgroundColor: ['green', 'red','yellow'],
        },
      ],
    };
    this.chartOptionsRealised = {
      responsive: true,
      plugins: {
        legend: { position: 'right' },
        title: { display: true, text: 'Realised Profit and Loss from stocks'}
      },
    };
  }
  else if(type == 'UnRealised'){
    this.chartDataUnRealised = {
      labels: ['Profit', 'Loss', 'OverallPL'],
      datasets: [
        {
          data: [profit,loss,overallpl],
          backgroundColor: ['green', 'red','yellow'],
          hoverBackgroundColor: ['green', 'red','yellow'],
        },
      ],
    };
    this.chartOptionsUnRealised = {
      responsive: true,
      plugins: {
        legend: { position: 'right' },
        title: { display: true, text: 'Un-Realised Profit and Loss from stocks'}
      },
    };
  }
}


getbarChart(){
  const documentStyle = getComputedStyle(document.documentElement);
  const textColor = documentStyle.getPropertyValue('--text-color');
  const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
  const surfaceBorder = documentStyle.getPropertyValue('--surface-border');
  this.basicData = {
    labels: ['Q1', 'Q2', 'Q3', 'Q4'],
    datasets: [
        {
            label: 'Sales',
            data: [540, 325, 702, 620],
            backgroundColor: ['rgba(255, 159, 64, 0.2)', 'rgba(75, 192, 192, 0.2)', 'rgba(54, 162, 235, 0.2)', 'rgba(153, 102, 255, 0.2)'],
            borderColor: ['rgb(255, 159, 64)', 'rgb(75, 192, 192)', 'rgb(54, 162, 235)', 'rgb(153, 102, 255)'],
            borderWidth: 1
        }
    ]
};

this.basicOptions = {
    plugins: {
        legend: {
            labels: {
                color: textColor
            }
        }
    },
    scales: {
        y: {
            beginAtZero: true,
            ticks: {
                color: textColorSecondary
            },
            grid: {
                color: surfaceBorder,
                drawBorder: false
            }
        },
        x: {
            ticks: {
                color: textColorSecondary
            },
            grid: {
                color: surfaceBorder,
                drawBorder: false
            }
        }
    }
};
}

}
