import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzCodeEditorModule } from 'ng-zorro-antd/code-editor';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzMessageModule, NzMessageService } from 'ng-zorro-antd/message';
import { NzEmptyModule} from 'ng-zorro-antd/empty';
import { NzTabsModule } from 'ng-zorro-antd/tabs';
import {NzInputNumberModule} from 'ng-zorro-antd/input-number';
import {NzSwitchModule} from 'ng-zorro-antd/switch';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NzButtonModule,
    NzInputNumberModule,
    NzSwitchModule,
    NzTableModule,
    NzInputModule,
    NzSelectModule,
    NzEmptyModule,
    NzFormModule,
    NzCardModule,
    NzDividerModule,
    NzTabsModule,
    NzCodeEditorModule,
    NzIconModule,
    NzMessageModule,
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  transformations: any[] = [
  ];

  responseData: any = null;
  loading = false;
  activeTab = 0;

  constructor(private http: HttpClient, private message: NzMessageService) {}

  addTransformation() {
    this.transformations.push({
      "$type": "where",
      "condition": ""
    });
  }

  removeTransformation(index: number) {
    this.transformations.splice(index, 1);
  }

  executeTransformations() {
    this.loading = true;
    this.http.post('/api/transform', { transformations: this.transformations })
      .subscribe({
        next: (response: any) => {
          this.responseData = response;
          this.loading = false;
          this.message.success('Request successful');
        },
        error: (error) => {
          this.loading = false;
          this.message.error('Request failed');
          console.error(error);
        }
      });
  }

  addMappingField(transformation: any) {
    if (!transformation.mapping) {
      transformation.mapping = {};
    }
    transformation.mapping['new_field'] = '';
  }

  getObjectKeys(obj: any): string[] {
    return Object.keys(obj || {});
  }

  trackByIndex(index: number): number {
    return index;
  }

  protected readonly Object = Object;
}
