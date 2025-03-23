import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FileVaultComponent } from './file-vault.component';

describe('FileVaultComponent', () => {
  let component: FileVaultComponent;
  let fixture: ComponentFixture<FileVaultComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [FileVaultComponent]
    });
    fixture = TestBed.createComponent(FileVaultComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
