import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { User } from '../_models/user';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/message';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsers(page?, itemsPerPage?, userParams?, likesParam?): Observable<PaginatedResult<User[]>> {
    const paginatedResult = new PaginatedResult<User[]>();
    let params = new HttpParams();

    if (page !== null && itemsPerPage !== null)  {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    if (userParams != null) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }

    if (likesParam === 'Likers') {
      params = params.append('likers', 'true');
    }

    if (likesParam === 'Likees') {
      params = params.append('likees', 'true');
    }

    const url = this.baseUrl + 'users';

    return this.http.get<User[]>(url, { observe: 'response', params })
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') != null) {
            const parsed = JSON.parse(response.headers.get('Pagination'));
            paginatedResult.pagination = parsed;
          }

          return paginatedResult;
        })
      );
  }

  getUser(id): Observable<User> {
    const url = this.baseUrl + 'users/' + id;

    return this.http.get<User>(url);
  }

  updateUser(id: number, user: User) {
    const url =  this.baseUrl + 'users/' + id;

    return this.http.put(url, user);
  }

  setMainPhoto(userId: number, id: number) {
    const url = this.baseUrl + 'users/' + userId + '/photos/' + id + '/setMain';

    return this.http.post(url, {});
  }

  deletePhoto(userId: number, id: number) {
    const url = this.baseUrl + 'users/' + userId + '/photos/' + id;

    return this.http.delete(url);
  }

  sendLike(id: number, recipientId: number) {
    const url = this.baseUrl + 'users/' + id + '/like/' + recipientId;

    return this.http.post(url, {});
  }

  getMessages(id: number, page?, itemsPerPage?, messageContainer?) {
    const paginatedResult = new PaginatedResult<Message[]>();
    let params = new HttpParams();
    params = params.append('MessageContainer', messageContainer);

    if (page !== null && itemsPerPage !== null)  {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    const url = this.baseUrl + 'users/' + id + '/messages';

    return this.http.get<Message[]>(url, {observe: 'response', params})
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') !== null) {
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }

          return paginatedResult;
        })
      );
  }

  getMessageThread(id: number, recipientId: number) {
    const url = `${this.baseUrl}users/${id}/messages/thread/${recipientId}`;

    return this.http.get<Message[]>(url);
  }

  sendMessage(id: number, message: Message) {
    const url = `${this.baseUrl}users/${id}/messages`;

    return this.http.post(url, message);
  }

  deleteMessage(id: number, userId: number) {
    const url = `${this.baseUrl}users/${userId}/messages/${id}`;

    return this.http.post(url, {});
  }

  markAsRead(userId: number, messageId: number) {
    const url = `${this.baseUrl}users/${userId}/messages/${messageId}/read`;

    return this.http.post(url, {})
      .subscribe();
  }
}
