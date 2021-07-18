import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { UserLoginResponse } from '../_models/userLoginResponse';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginationHeaders, getPaginationResult } from './paginationHelper';
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
  members: Member[] = [];
  memberCache = new Map();
  private userParams: UserParams;
  private user: UserLoginResponse;

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUserLoggedIn$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(this.user);
    })
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }

  getMembers(userParams: UserParams) { // }: Observable<Member[]> {
    const key = Object.values(userParams).join('-');
    const response = this.memberCache.get(key);
    if(response) {
      return of(response);
    }

    let params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return getPaginationResult<Member[]>(this.http, `${this.baseUrl}/user`, params).pipe(
      map(result => {
        this.memberCache.set(key, result);
        return result;
      })
    );
  }

  getMemberByUserName(userName: string) : Observable<Member> {
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((x: Member) => x.userName === userName);
    if(member) return of(member);

    return this.http.get<Member>(`${this.baseUrl}/user/${userName}`);
  }

  updateMember(member: Member) {
    return this.http.put(`${this.baseUrl}/user`, member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(`${this.baseUrl}/user/set-main-photo/${photoId}`, {});
  }

  deletePhtot(photoId: number) {
    return this.http.delete(`${this.baseUrl}/user/delete-photo/${photoId}`);
  }

  addLike(userName: string) {
    return this.http.post(`${this.baseUrl}/likes/${userName}`, {});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    const params = getPaginationHeaders(pageNumber, pageSize);
    params.append('predicate', predicate);

    return getPaginationResult<Partial<Member[]>>(this.http, `${this.baseUrl}/likes`, params);
  }
}
