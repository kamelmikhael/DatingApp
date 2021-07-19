import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { UsersWithRolesDto } from '../_models/usersWithRolesDto';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsersWithRoles() {
    return this.http.get<Partial<UsersWithRolesDto[]>>(`${this.baseUrl}/admin/users-with-roles`);
  }

  updateUserRoles(userName: string, roles: string[]) {
    return this.http.post(`${this.baseUrl}/Admin/edit-roles/${userName}?roles=${roles}`, {});
  }
}
