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

  viewFile(fileId: any) {
    this.fileService.viewFile(fileId);
  }


  downloadFile(fileId: any): void {
    this.fileService.downloadFile(fileId).subscribe({
      next: (blob) => {
        // ✅ Create a URL from the blob
        const url = window.URL.createObjectURL(blob);
        
        // ✅ Create a temporary <a> element to trigger the download
        const a = document.createElement('a');
        a.href = url;
        a.download = `file_${fileId}`; // Adjust to actual filename if needed
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        
        // ✅ Revoke the object URL after download
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading file:', error);
      },
    });
  }
  
  
  deleteFile(fileId: any) {
    if (confirm(`Are you sure you want to delete ${fileId}?`)) {
      this.fileService.deleteFile(fileId).subscribe({
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
