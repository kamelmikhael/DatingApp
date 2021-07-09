import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) {}

  // Here we can intercept the request or response
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError(error => {
        if(error) {
          switch (error.status) {
            case 400:
              if(error.error.errors) { // 400 Validation error handler
                const modelStateErrors = [];
                for(const key in error.error.errors) {
                  if(error.error.errors[key]) {
                    modelStateErrors.push(error.error.errors[key]);
                  }
                }
                throw modelStateErrors.flat();
              } else if(typeof(error.error) === 'object') { // 400 - bad request
                this.toastr.error(error.statusText, error.status);
              } else {
                this.toastr.error(error.error, error.status);
              }
              break;
            case 401: // unauthorized
              this.toastr.error(error.statusText, error.status);
              break;
            case 404: // Not found
              this.router.navigateByUrl('/not-found');
              break;
            case 500: // internal server error
              const navigationExtras: NavigationExtras = {state: {error: error.error}};
              this.router.navigateByUrl('server-error', navigationExtras);
              break;
            default:
              this.toastr.error('Something unexpected goes wrong!');
              console.log(error);
              break;
          }
        }
        return throwError(error);
      })
    );
  }
}
