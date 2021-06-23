import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
// import { UserLoginResponse } from '../_models/userLoginResponse';

// const httpOptions = {
//   headers: new HttpHeaders({
//     Authorization: `Bearer ${(JSON.parse(localStorage.getItem(environment.userLocalstorageKey)) as UserLoginResponse)?.token}`
//   })
// };

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMembers() : Observable<Member[]> {
    return this.http.get<Member[]>(`${this.baseUrl}/user`);
  }

  getMemberByUserName(userName: string) : Observable<Member> {
    return this.http.get<Member>(`${this.baseUrl}/user/${userName}`);
  }
}
