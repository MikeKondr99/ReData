import {Component, input, signal} from '@angular/core';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzButtonModule} from 'ng-zorro-antd/button';


@Component({
  selector: 'app-collapse',
  standalone: true,
  imports: [
    NzIconModule,
    NzButtonModule,
  ],
  template: `
    <div>{{ header() }}
      <button nz-button nzType="link" nzShape="circle" nzSize="small" (click)="toggle()" >
        <nz-icon nzType="down" [nzRotate]="opened() ? 0 : 90"/>
      </button>
    </div>
    @if(opened()) {
      <div>
        <ng-content/>
      </div>
    }
  `
})
export class CollapseComponent {

  public header = input.required();

  public opened = signal(false)

  public toggle()
  {
    this.opened.update(o => !o);
  }

}
