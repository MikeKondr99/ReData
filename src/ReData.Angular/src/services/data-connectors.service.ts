import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DataConnectorListItem} from '../types';
import {toSignal} from '@angular/core/rxjs-interop';
import {BehaviorSubject, switchMap} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DataConnectorsService {
  private http = inject(HttpClient);
  private refresh$ = new BehaviorSubject<void>(undefined);

  dataConnectors = toSignal(
    this.refresh$.pipe(
      switchMap(() => this.http.get<DataConnectorListItem[]>('api/data-connectors'))
    ),
    { initialValue: [] }
  );

  refresh() {
    this.refresh$.next();
  }
}

