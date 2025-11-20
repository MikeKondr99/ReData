import {Component, inject, output, model, input, effect, untracked} from '@angular/core';
import {NzInputModule} from 'ng-zorro-antd/input';
import {FormsModule} from '@angular/forms';
import {NzAutocompleteModule} from 'ng-zorro-antd/auto-complete';
import {HttpClient} from '@angular/common/http';
import {ApiResponse, DataConnectorListItem} from '../types';
import {toSignal} from '@angular/core/rxjs-interop';
import {NzSelectModule} from 'ng-zorro-antd/select';
import {DataConnectorsService} from '../services/data-connectors.service';

@Component({
  selector: 'app-data-connector-selector',
  imports: [
    NzInputModule,
    NzSelectModule,
    FormsModule,
    NzAutocompleteModule,

  ],
  template: `
    <div>
      <nz-select nzShowSearch nzAllowClear nzPlaceHolder="Select a person" [(ngModel)]="model">
        @for (connector of dataConnectors(); track connector.id) {
          <nz-option [nzValue]="connector" [nzLabel]="connector.name">
            {{ connector.name }}
          </nz-option>
        }
      </nz-select>
    </div>
  `,
  styles: ``,
})
export class DataConnectorSelectorComponent {

  private connectors = inject(DataConnectorsService);

  public id = input.required<string | undefined>();

  _ = effect(_ => {
    let id = this.id() ?? '00000000-0000-0000-0000-000000000000';
    let connectors = this.dataConnectors();
    let connector = connectors?.find(c => c.id === id)
    if(connector) {
      this.model.set(connector);
    }
  })

  public model = model<DataConnectorListItem>();

  dataConnectors = this.connectors.dataConnectors;
}
