import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { UsersWithRolesDto } from 'src/app/_models/usersWithRolesDto';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: Partial<UsersWithRolesDto[]>;
  modalRef: BsModalRef;

  constructor(private adminService: AdminService,
    private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe(users => {
      this.users = users;
    })
  }

  openRolesModal(userToUpdate: UsersWithRolesDto) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user: userToUpdate,
        roles: this.getRolesArray(userToUpdate),
      }
    }

    this.modalRef = this.modalService.show(RolesModalComponent, config);

    this.modalRef.content.updateSelectedRoles.subscribe(values => {
      const rolesToUpdate = {
        roles: [...values.filter(el => el.checked === true).map(el => el.name)]
      }

      if(rolesToUpdate) {
        this.adminService.updateUserRoles(userToUpdate.userName, rolesToUpdate.roles).subscribe(() => {
          userToUpdate.roles = [...rolesToUpdate.roles];
        })
      }
    });
  }

  private getRolesArray(user: UsersWithRolesDto) {
    const roles = [];
    const userRoles = user.roles;
    const avaliableRoles: any[] = [
      {name: 'Admin', value: 'Admin'},
      {name: 'Moderator', value: 'Moderator'},
      {name: 'Member', value: 'Member'},
    ];

    avaliableRoles.forEach(role => {
      if(userRoles.includes(role.name)) {
        role.checked = true;
      } else {
        role.checked = false;
      }
      roles.push(role);
    });

    return roles;
  }

}
