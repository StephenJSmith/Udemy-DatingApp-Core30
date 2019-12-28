import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MemberEditResolver implements Resolve<User> {

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private router: Router,
    private alertify: AlertifyService
  ) {}

  resolve(route: ActivatedRouteSnapshot): Observable<User> {
    const routeId = this.authService.decodedToken.nameid;

    return this.userService.getUser(routeId)
      .pipe(
        catchError(err => {
          this.alertify.error('Problem retrieving your data');
          this.router.navigate(['/members']);

          return of(null);
        })
      );
  }
}
