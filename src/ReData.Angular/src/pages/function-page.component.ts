import {Component} from '@angular/core';
import {FunctionsComponent} from '../components/functions.component';

@Component({
  selector: 'app-functions-page',
  standalone: true,
  imports: [
    FunctionsComponent
  ],
  template: `
    <div class="py-3 px-7">
      <app-functions></app-functions>
    </div>
  `,
  styles: ``
})
export class FunctionsPageComponent {
}
