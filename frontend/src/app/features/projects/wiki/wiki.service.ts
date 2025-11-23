import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface WikiPage {
  id: string;
  projectId: string;
  parentId?: string;
  title: string;
  content: string;
  createdAt: Date;
  updatedAt: Date;
  lastEditorId?: string;
  lastEditorName?: string;
  children?: WikiPage[];
}

export interface CreateWikiPageDto {
  title: string;
  content: string;
  parentId?: string;
}

export interface UpdateWikiPageDto {
  title: string;
  content: string;
  parentId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class WikiService {
  private apiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) { }

  getProjectWiki(projectId: string): Observable<WikiPage[]> {
    return this.http.get<WikiPage[]>(`${this.apiUrl}/wiki/project/${projectId}`);
  }

  getPage(id: string): Observable<WikiPage> {
    return this.http.get<WikiPage>(`${this.apiUrl}/wiki/${id}`);
  }

  createPage(projectId: string, dto: CreateWikiPageDto): Observable<WikiPage> {
    return this.http.post<WikiPage>(`${this.apiUrl}/wiki/project/${projectId}`, dto);
  }

  updatePage(id: string, dto: UpdateWikiPageDto): Observable<WikiPage> {
    return this.http.put<WikiPage>(`${this.apiUrl}/wiki/${id}`, dto);
  }

  deletePage(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/wiki/${id}`);
  }
}
