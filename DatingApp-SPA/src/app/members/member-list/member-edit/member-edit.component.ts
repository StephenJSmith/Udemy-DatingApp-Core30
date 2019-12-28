import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { User } from '../../../_models/user';
import { AlertifyService } from '../../../_services/alertify.service';
import { AuthService } from '../../../_services/auth.service';
import { UserService } from '../../../_services/user.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm', { static: true }) editForm: NgForm;
  user: User;
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(
    private route: ActivatedRoute,
    private alertify: AlertifyService,
    private userService: UserService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data.user;
    });
  }

  updateUser() {
    const decodedTokenId = this.authService.decodedToken.nameid;
    this.userService.updateUser(decodedTokenId, this.user)
      .subscribe(next => {
        this.alertify.success('Profile updated successfully');
        this.editForm.reset(this.user);
      }, error => {
        this.alertify.error(error);
      });
  }
}
