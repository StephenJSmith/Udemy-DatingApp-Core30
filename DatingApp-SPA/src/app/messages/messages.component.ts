import { Component, OnInit } from '@angular/core';
import { Pagination } from '../_models/pagination';
import { ActivatedRoute } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Message } from '../_models/message';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  messageContainer = 'Unread';

  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private authService: AuthService,
    private alertify: AlertifyService
  ) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.messages = data.messages.result;
      this.pagination = data.messages.pagination;
    }, error => {
      this.alertify.error(error);
    });
  }

  loadMessages() {
    const userId = this.authService.decodedToken.nameid;
    const { currentPage, itemsPerPage } = this.pagination;
    this.userService.getMessages(userId, currentPage,
      itemsPerPage, this.messageContainer)
      .subscribe((res: any) => {
        this.messages = res.result;
        this.pagination = res.pagination;
      }, error => {
        this.alertify.error(error);
      });
  }

  deleteMessage(id: number) {
    this.alertify.confirm('Are you sure you want to delete this message?', () => {
      const userId = this.authService.decodedToken.nameid;
      this.userService.deleteMessage(id, userId)
        .subscribe(() => {
          const index = this.messages.findIndex(m => m.id === id);
          this.messages.splice(index, 1);
          this.alertify.success('Message has been deleted');
        }, error => {
          this.alertify.error('Failed to delete the message');
        });
    });
  }

  pageChanged(event: any) {
    this.pagination.currentPage = event.page;
    this.loadMessages();
  }
}
