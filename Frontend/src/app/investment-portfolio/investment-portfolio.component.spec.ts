import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InvestmentPortfolioComponent } from './investment-portfolio.component';

describe('InvestmentPortfolioComponent', () => {
  let component: InvestmentPortfolioComponent;
  let fixture: ComponentFixture<InvestmentPortfolioComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [InvestmentPortfolioComponent]
    });
    fixture = TestBed.createComponent(InvestmentPortfolioComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
