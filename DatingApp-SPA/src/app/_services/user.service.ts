import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsers(): Observable<User[]> {
    const url = this.baseUrl + 'users';

    return this.http.get<User[]>(url);
  }

  getUser(id): Observable<User> {
    const url = this.baseUrl + 'users/' + id;

    return this.http.get<User>(url);
  }

  updateUser(id: number, user: User) {
    const url =  this.baseUrl + 'users/' + id;

    return this.http.put(url, user);
  }
}
