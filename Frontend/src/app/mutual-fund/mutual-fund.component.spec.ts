import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MutualFundComponent } from './mutual-fund.component';

describe('MutualFundComponent', () => {
  let component: MutualFundComponent;
  let fixture: ComponentFixture<MutualFundComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MutualFundComponent]
    });
    fixture = TestBed.createComponent(MutualFundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
