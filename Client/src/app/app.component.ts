import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users: any = [];

  constructor(private http: HttpClient, private accountService: AccountService) { }

  ngOnInit(): void {
    this.getUsers();
    this.setCurrentUserSource();
  }

  getUsers() {
    this.http.get('https://localhost:5001/api/User').subscribe((response => {
      console.log(response);
      this.users = response;
    }), (error) => {
      console.log(error);
    });
  }

  setCurrentUserSource() {
    const user = this.accountService.getCurrentUserFromLocalStorage();
    this.accountService.setCurrentUserSource(user);
  }
}
