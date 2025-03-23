import {Component, computed, inject, signal} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {toSignal} from '@angular/core/rxjs-interop';
import {NzTableModule} from 'ng-zorro-antd/table';
import {NzDividerModule} from 'ng-zorro-antd/divider';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {DataSource} from '../types';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {
  FormArray,
  FormControl,
  FormGroup,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators, ValueChangeEvent
} from '@angular/forms';
import {NzInputModule} from 'ng-zorro-antd/input';
import {NzFormModule} from 'ng-zorro-antd/form';
import {NzAutocompleteModule} from 'ng-zorro-antd/auto-complete';

@Component({
  standalone: true,
  imports: [NzTableModule, NzDividerModule, NzIconModule, NzButtonModule, ReactiveFormsModule, NzInputModule, NzFormModule, NzAutocompleteModule],
  selector: 'app-edit-datasource-form',
  template: `
    <form [formGroup]="form">
      @let props = form.controls;
      <nz-form-item>
        <nz-form-label [nzSm]="6" [nzXs]="24" nzRequired nzFor="name">Name</nz-form-label>
        <nz-form-control [nzSm]="14" [nzXs]="24">
          <input nz-input [formControl]="props.name" id="email" />
        </nz-form-control>
      </nz-form-item>
      <nz-form-item>
        <nz-form-label [nzSm]="6" [nzXs]="24" nzFor="description">Description</nz-form-label>
        <nz-form-control [nzSm]="14" [nzXs]="24">
          <input nz-input [formControl]="props.description" id="description" />
        </nz-form-control>
      </nz-form-item>
      <nz-form-item>
        <nz-form-label [nzSm]="6" [nzXs]="24" nzRequired nzFor="type">Type</nz-form-label>
        <nz-form-control [nzSm]="14" [nzXs]="24">
          <input nz-input [formControl]="props.type" id="type" />
        </nz-form-control>
      </nz-form-item>
      <nz-form-item>
        <nz-form-label [nzSm]="6" [nzXs]="24">Parameters</nz-form-label>
      </nz-form-item>
      <span [formArrayName]="'parameters'">
        @for (par of form.controls.parameters.controls; track par; let i = $index) {
          <nz-form-item>
            <nz-form-label [nzSm]="6" [nzXs]="24" nzNoColon></nz-form-label>
            <nz-form-control [nzSm]="14" [nzXs]="24">
              <div class="flex gap-2">
                <input nz-input [nzAutocomplete]="auto" [formControl]="par.controls.key" (change)="keyChanged(i,par.controls.key.value)"/>
                <nz-autocomplete [nzDataSource]="options()" nzBackfill #auto></nz-autocomplete>
                :
                <input nz-input [formControl]="par.controls.value"/>
              </div>
            </nz-form-control>
          </nz-form-item>
        }
      </span>
      <nz-form-item>
        <nz-form-label [nzSm]="6" [nzXs]="24" nzNoColon></nz-form-label>
        <nz-form-control [nzSm]="14" [nzXs]="24">
          <button nz-button nzType="primary" (click)="addField()">Submit</button>
        </nz-form-control>
      </nz-form-item>
    </form>
   `,
})
export class EditDatasourceFormComponent {

  formBuilder = inject(NonNullableFormBuilder);

  possible = ["Host", "Password", "Username"]

  keys = signal<string[]>([]);

  options = computed(() => {
    let op = this.possible.filter(k => !this.keys().includes(k))
    console.log(op)
    return op;
  });


  form = this.formBuilder.group({
    name: [null, Validators.required],
    description: [null, Validators.required],
    type: [null, Validators.required],
    parameters: this.formBuilder.array<FormGroup<KeyValueForm>>([]),
  })

  addField() {
    this.form.controls.parameters.push(this.formBuilder.group({
      key: ['', Validators.required],
      value: ['', Validators.required],
    }))
    this.keys.update(ks => [...ks, '']);
  }

  keyChanged(index: number, newValue: string) {
    console.log(`key[${index}] = ${newValue}`)
    let keysCopy = [...this.keys()];
    keysCopy[index] = newValue;
    this.keys.set(keysCopy);
  }
}

interface KeyValueForm
{
  key: FormControl<string>
  value: FormControl<string>
}
