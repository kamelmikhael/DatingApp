import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { UserRegisterModel } from '../_models/userRegisterModel';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelClick = new EventEmitter();

  model: UserRegisterModel = {userName: '', password: ''};

  constructor(private accountService: AccountService, private toastr: ToastrService) { }

  ngOnInit() {
  }

  register() {
    this.accountService.register(this.model).subscribe(response => {
      console.log(response);
      this.cancel();
    }, error => {
      console.log(error);
      this.toastr.error(error.error);
    });
  }

  cancel() {
    this.cancelClick.emit(false);
  }

}
