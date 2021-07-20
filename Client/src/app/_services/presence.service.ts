import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { UserLoginResponse } from '../_models/userLoginResponse';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService,
    private router: Router) { }

  createHubConnection(user: UserLoginResponse) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}/presence`, {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', userName => {
      // this.toastr.info(`${userName} has connected`);
      this.onlineUsers$.pipe(take(1)).subscribe(userNames => {
        this.onlineUsersSource.next([...userNames, userName]);
      })
    });

    this.hubConnection.on('UserIsOffline', userName => {
      // this.toastr.warning(`${userName} has disconnected`);
      this.onlineUsers$.pipe(take(1)).subscribe(userNames => {
        this.onlineUsersSource.next([...userNames.filter(x => x !== userName)]);
      })
    });

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
    });

    this.hubConnection.on('NewMessageReceived', ({userName, knownAs}) => {
      this.toastr.info(`${knownAs} has sent you a new message`)
        .onTap
        .pipe(take(1))
        .subscribe(() => this.router.navigate(['/members', userName], {queryParams: {tabId: 3}}));
    });
  }

  stopHubConnection() {
    if(this.hubConnection.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }
}
