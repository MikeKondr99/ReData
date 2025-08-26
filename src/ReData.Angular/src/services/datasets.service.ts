import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {ApiResponse, DataSetViewModel, FunctionViewModel} from '../types';
import {toSignal} from '@angular/core/rxjs-interop';
import {BehaviorSubject, switchMap} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DatasetsService {
  private http = inject(HttpClient);
  private refresh$ = new BehaviorSubject<void>(undefined);

  datasets = toSignal(
    this.refresh$.pipe(
      switchMap(() => this.http.get<DataSetViewModel[]>('api/datasets'))
    ),
    { initialValue: [] }
  );

  getById(id: string) {
    return toSignal(
      this.http.get<DataSetViewModel>(`api/datasets/${encodeURIComponent(id)}`),
      { initialValue: null }
    );
  }

  refresh() {
    this.refresh$.next();
  }
}

