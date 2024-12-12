import { Component, OnInit } from '@angular/core';
import { FileService } from '../file.service';
import { HttpClient } from '@angular/common/http';


@Component({
  selector: 'app-file-vault',
  templateUrl: './file-vault.component.html',
  styleUrls: ['./file-vault.component.css']
})
export class FileVaultComponent implements OnInit {
  files: any[] = [];

  constructor(private fileService: FileService,public http:HttpClient) {}

  ngOnInit(): void {
    this.loadFiles();
  }

  loadFiles(): void {
    this.fileService.getFiles().subscribe({
      next: (data) => {
        this.files = data;
        console.log(this.files);
        
      },
      error: (error) => {
        console.error('Error fetching files:', error);
      },
    });
  }

  viewFile(filePath: string) {
    this.fileService.viewFile(filePath);
  }

  downloadFile(fileName: string): void {
    this.fileService.downloadFile(fileName).subscribe({
      next: (response) => {
        const blob = new Blob([response], { type: 'application/octet-stream' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading file:', error);
      },
    });
  }


  deleteFile(fileName: string) {
    if (confirm(`Are you sure you want to delete ${fileName}?`)) {
      this.fileService.deleteFile(fileName).subscribe({
        next: () => {
          alert('File deleted successfully.');
          this.loadFiles(); // Refresh the list
        },
        error: (err) => {
          console.error('Error deleting file', err);
        }
      });
    }
  }
}
