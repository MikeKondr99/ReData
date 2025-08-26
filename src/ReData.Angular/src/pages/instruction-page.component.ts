import {Component} from '@angular/core';
import {InstructionComponent} from '../components/instruction.component';

@Component({
  selector: 'app-instruction-page',
  standalone: true,
  imports: [
    InstructionComponent
  ],
  template: `
    <div class="py-5 px-16 w-[50%]">
        <app-instruction class=>
        </app-instruction>
    </div>
  `,
  styles: ``
})
export class InstructionPageComponent {
}
