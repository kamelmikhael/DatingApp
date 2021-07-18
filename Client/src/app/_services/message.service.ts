import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginationHeaders, getPaginationResult } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginationResult<Message[]>(this.http, `${this.baseUrl}/messages`, params);
  }

  getMessageThread(userName: string) {
    return this.http.get<Message[]>(`${this.baseUrl}/messages/thread/${userName}`);
  }

  sendMessage(userName: string, content: string) {
    const body = { receipientUserName: userName, content};
    return this.http.post<Message>(`${this.baseUrl}/messages`, body);
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}/messages/${id}`);
  }
}
