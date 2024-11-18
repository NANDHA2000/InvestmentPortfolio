import { HttpClient } from '@angular/common/http';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent {
  chartData: any;
  chartOptions: any;
  portfolioData: any[] = [];
  totalProfit: number=0;
  totalLoss: number=0;

  constructor(private http: HttpClient) {}


  ngOnInit(){
    this.getInvestmentData()
  }

  getInvestmentData() {
    this.http.get<any[]>('https://localhost:44394/api/Excel/InvestmentData').subscribe(
      res => {
        console.log("Response data:", res);
        this.portfolioData = res; // Ensure that the response is an array
        const values = res.map((item) => item.realisedPL);

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
          console.log(this.totalProfit,this.totalLoss);
          this.getpieChart(this.totalProfit,this.totalLoss)
      },
      error => {
        console.error("Error:", error);
      }
    );
}

getpieChart(profit:number,loss:number){
  this.chartData = {
    labels: ['Profit', 'Loss'],
    datasets: [
      {
        data: [profit,loss],
        backgroundColor: ['green', 'red'],
        hoverBackgroundColor: ['#8FFE09', '#FF474D'],
      },
    ],
  };
  this.chartOptions = {
    responsive: true,
    plugins: {
      legend: { position: 'right' },
      title: { display: true, text: 'Profit and Loss from stocks'}
    },
  };
}

}
