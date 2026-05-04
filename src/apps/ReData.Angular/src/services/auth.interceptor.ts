import {HttpInterceptorFn} from '@angular/common/http';
import {inject} from '@angular/core';
import {from} from 'rxjs';
import {switchMap} from 'rxjs/operators';
import {AuthService} from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return from(auth.getToken()).pipe(
    switchMap((token) => {
      if (!token) {
        return next(req);
      }

      return next(req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
        }
      }));
    })
  );
};
