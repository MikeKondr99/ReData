import {Component, inject} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {toSignal} from '@angular/core/rxjs-interop';
import {NzTableModule} from 'ng-zorro-antd/table';
import {NzDividerModule} from 'ng-zorro-antd/divider';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {DataSource} from '../types';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {NzModalService} from 'ng-zorro-antd/modal';
import {EditDatasourceFormComponent} from './edit-datasource-form';

@Component({
  standalone: true,
  imports: [NzTableModule, NzDividerModule, NzIconModule, NzButtonModule, EditDatasourceFormComponent],
  selector: 'app-data-source-table',
  template: `
    <div>
      <button nz-button nzType="primary">
        Add
      </button>

    </div>
    <nz-table #basicTable [nzData]="dataSources()">
      <thead>
      <tr>
        <th nzWidth="0px">Type</th>
        <th nzWidth="20%">Name</th>
        <th>Description</th>
        <th nzRight nzWidth="0px">Actions</th>
      </tr>
      </thead>
      <tbody>
        @for (dataSource of dataSources(); track dataSource.id) {
          <tr>
            <td>{{dataSource.type}}</td>
            <td>{{dataSource.name}}</td>
            <td>{{dataSource.description}}</td>
            <td nzRight class="flex gap-1">
              <button nz-button (click)="editDataSource(dataSource)">
                <span nz-icon nzType="edit" nzTheme="outline"></span>
              </button>
              <button nz-button  nzDanger>
                <span nz-icon nzType="delete" nzTheme="outline"></span>
              </button>
            </td>
          </tr>

        }
      </tbody>
    </nz-table>
   `,
})
export class DataSourceTableComponent {

  httpClient = inject(HttpClient);

  public editDataSource(dataSource: DataSource) {
    console.log(dataSource)
  }
  showModal2(): void {
  }

  public deleteDataSource(dataSource: DataSource) {
    console.error("deleteDataSource not implemented");
  }

  dataSources = toSignal(this.httpClient.get<DataSource[]>('/api/datasource'), {initialValue: <DataSource[]>[]});

}
