import { HttpClient } from '@angular/common/http';
import { Component, Input } from '@angular/core';
import { SharedService } from '../shared/shared.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent {
  chartData: any;
  chartOptions: any;
  //portfolioData: any[] = [];
  totalProfit: number=0;
  totalLoss: number=0;
  portfolioData : any[] = [];

  basicData: any;

  basicOptions: any;
  GetData: any;

  constructor(private http: HttpClient,private shared:SharedService) {}


  ngOnInit():void{
    this.getInvestmentData();
    this.getbarChart();
    debugger
    const dataString = sessionStorage.getItem("Data")
    const data = dataString ? JSON.parse(dataString) : null;
    console.log(data);

    // if (Array.isArray(data)) {
    //   // Check if data is in the expected format (array) before passing it
    // // const period = this.shared.calculateHoldingPeriods(data);
    // // console.log(period);
     
      
    // } else {
    //   console.error("Data is not in the expected format of an array.");
    // }
    
    
    
  }

  getInvestmentData() {
    this.http.get<any[]>('https://localhost:44394/api/Excel/InvestmentData').subscribe(
      res => {
        console.log("Response data:", res);
        //this.portfolioData = res; // Ensure that the response is an array
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

          let OverallPL = this.totalProfit - Math.abs(this.totalLoss)
          this.getpieChart(this.totalProfit,this.totalLoss,OverallPL)
      },
      error => {
        console.error("Error:", error);
      }
    );
}

getpieChart(profit:number,loss:number,overallpl:number){
  this.chartData = {
    labels: ['Profit', 'Loss'],
    datasets: [
      {
        data: [profit,loss,overallpl],
        backgroundColor: ['green', 'red','yellow'],
        hoverBackgroundColor: ['green', 'red','yellow'],
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
