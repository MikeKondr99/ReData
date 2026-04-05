import {inject, Injectable} from '@angular/core';
import {toSignal} from '@angular/core/rxjs-interop';
import {DefaultService} from '../api/api/default.service';

@Injectable({
  providedIn: 'root'
})
export class FunctionService {

  defaultService = inject(DefaultService);

  data = toSignal(this.defaultService.getAllFunctions());
}


