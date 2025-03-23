import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { URL_LIST } from '../Config/url.config';

@Injectable({
  providedIn: 'root',
})
export class FileService {

  constructor(private http: HttpClient) {}

  getFiles() {
    return this.http.get<any[]>(`${URL_LIST.GET_FILES}`);
  }

  viewFile(fileName: string) {
    // Assuming the backend URL is configured correctly
    const filePath = `${URL_LIST.VIEW_FILE}/${fileName}`;
    window.location.href = filePath; // This will open the file in the same tab
  }
  
  

  downloadFile(fileName: string) {
    return this.http.get(`${URL_LIST.DOWNLOAD_FILE}/${fileName}`, {
      responseType: 'blob',
    });

    
  }

  deleteFile(fileName: string): Observable<any> {
    return this.http.delete(`${URL_LIST.DELETE_FILE}/${fileName}`);
  }
}

