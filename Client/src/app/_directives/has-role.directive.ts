import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs/operators';
import { UserLoginResponse } from '../_models/userLoginResponse';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]' // *appHasRole="['Admin', 'Moderator']"
})
export class HasRoleDirective implements OnInit{
  @Input() appHasRole: string[];
  user: UserLoginResponse;

  constructor(private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private accountService: AccountService) {
    this.accountService.currentUserLoggedIn$.pipe(take(1)).subscribe(user => {
      this.user = user;
    })
  }

  ngOnInit(): void {
    // Clear the view if no roles
    if(this.user == null || !this.user?.roles) {
      this.viewContainerRef.clear();
      return;
    }

    if(this.user?.roles.some(r => this.appHasRole.includes(r))) {
      this.viewContainerRef.createEmbeddedView(this.templateRef)
    } else {
      this.viewContainerRef.clear();
    }
  }

}
