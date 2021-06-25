import { Component, Input, OnInit } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Photo } from 'src/app/_models/photo';
import { UserLoginResponse } from 'src/app/_models/userLoginResponse';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() member: Member;
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  // hasAnotherDropZoneOver:boolean;
  // response:string;
  baseUrl = environment.apiUrl;
  user: UserLoginResponse;

  constructor(private accountService: AccountService,
    private memberService: MembersService) {
    this.accountService.currentUserLoggedIn$.pipe(take(1)).subscribe(user => this.user = user);
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: `${this.baseUrl}/user/add-photo`,
      authToken: `Bearer ${this.user.token}`,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10*1024*1024, // 10 MB
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if(response) {
        const photo = JSON.parse(response);
        this.member.photos.push(photo);
      }
    };
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo.id).subscribe(() => {
      this.user.photoUrl = photo.url;
      this.accountService.setCurrentUserSource(this.user);
      this.member.photoUrl = photo.url;
      this.member.photos.forEach(p => {
        if(p.isMain === true) p.isMain = false;
        if(p.id === photo.id) p.isMain = true;
      })
    });
  }

  // deletePhoto(photo: Photo) {
  //   this.memberService.deletePhtot(photo.id).subscribe(() => {
  //     const index = this.member.photos.indexOf(photo);
  //     this.member.photos.splice(index, 1);
  //   })
  // }

  deletePhoto(photoId: number) {
    this.memberService.deletePhtot(photoId).subscribe(() => {
      this.member.photos = this.member.photos.filter(x => x.id !== photoId);
    })
  }

}
