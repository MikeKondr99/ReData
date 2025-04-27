import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {ApiResponse, FunctionViewModel} from '../types';
import {toSignal} from '@angular/core/rxjs-interop';

@Injectable({
  providedIn: 'root'
})
export class FunctionService {

  http = inject(HttpClient);

  data = toSignal(this.http.get<FunctionViewModel[]>('api/functions'));
}


