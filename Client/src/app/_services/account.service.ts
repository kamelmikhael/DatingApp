import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { map } from 'rxjs/operators';
import { UserLoginResponse } from '../_models/userLoginResponse';
import { ReplaySubject } from 'rxjs';
import { UserRegisterModel } from '../_models/userRegisterModel';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  // Store the current logged-in user
  private currentUserSource = new ReplaySubject<UserLoginResponse>(1);
  currentUserLoggedIn$ = this.currentUserSource.asObservable(); // contains user object or null

  constructor(private http: HttpClient) { }

  register(input: UserRegisterModel) {
    return this.http.post(`${environment.apiUrl}/account/register`, input).pipe(
      map((user: UserLoginResponse) => {
        if (user) {
          this.setCurrentUserSource(user);
        }
        // return user;
      })
    );
  }

  login(input: any) {
    return this.http.post(`${environment.apiUrl}/account/login`, input).pipe(
      map((user: UserLoginResponse) => {
        if (user) {
          this.setCurrentUserSource(user);
        }
        // return user;
      })
    );
  }

  logout() {
    localStorage.removeItem(environment.userLocalstorageKey);
    this.setCurrentUserSource(null);
  }

  setCurrentUserSource(user: UserLoginResponse) {
    if(user != null) {
      user.roles = [];
      const roles = this.getDecodedToken(user.token).role;
      if(Array.isArray(roles)) {
        user.roles = roles;
      } else {
        user.roles.push(roles);
      }
    }

    localStorage.setItem(environment.userLocalstorageKey, JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  getCurrentUserFromLocalStorage(): UserLoginResponse {
    return JSON.parse(localStorage.getItem(environment.userLocalstorageKey)) as UserLoginResponse;
  }

  getDecodedToken(token) {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
