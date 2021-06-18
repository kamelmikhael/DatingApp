import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SettingService {

  baseUrl = 'https://localhost:5001/api';

  constructor() { }
}
