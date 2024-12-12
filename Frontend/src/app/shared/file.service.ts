import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class FileService {
  private baseUrl = 'https://localhost:44394/api'; // Adjust based on your API URL

  constructor(private http: HttpClient) {}

  getFiles() {
    return this.http.get<any[]>(`${this.baseUrl}/Vault/files`);
  }

  viewFile(fileName: string) {
    // Assuming the backend URL is configured correctly
    const filePath = `${this.baseUrl}/files/view/${fileName}`;
    window.location.href = filePath; // This will open the file in the same tab
  }
  
  

  downloadFile(fileName: string) {
    return this.http.get(`${this.baseUrl}/Vault/download/${fileName}`, {
      responseType: 'blob',
    });
  }

  deleteFile(fileName: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/files/delete/${fileName}`);
  }
}
