import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SettingService } from './setting.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private setting: SettingService, private http: HttpClient) { }

  login(input: any) {
    return this.http.post(`${this.setting.baseUrl}/account/login`, input);
  }
}
