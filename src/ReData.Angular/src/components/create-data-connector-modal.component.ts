import {Component, inject, model, output} from '@angular/core';
import {NzModalComponent, NzModalContentDirective, NzModalModule} from 'ng-zorro-antd/modal';
import {NzFormModule} from 'ng-zorro-antd/form';
import {NzInputModule} from 'ng-zorro-antd/input';
import {FormsModule} from '@angular/forms';
import {NzCheckboxModule} from 'ng-zorro-antd/checkbox';
import {NzUploadFile, NzUploadModule} from 'ng-zorro-antd/upload';
import {NzIconModule} from 'ng-zorro-antd/icon';
import {NzButtonModule} from 'ng-zorro-antd/button';
import {filter} from 'rxjs';
import {HttpClient, HttpRequest, HttpResponse} from '@angular/common/http';
import {DataConnectorsService} from '../services/data-connectors.service';
import {DataConnectorListItem} from '../types';

@Component({
  selector: 'app-create-data-connector-modal',
  imports: [
    NzModalModule,
    NzUploadModule,
    NzFormModule,
    NzIconModule,
    NzInputModule,
    NzButtonModule,
    NzCheckboxModule,
    FormsModule,
  ],
  template: `
    <nz-modal [(nzVisible)]="isVisible" nzTitle="Создание коннектора" (nzOnCancel)="handleCancel()" (nzOnOk)="handleOk()" (nzAfterClose)="handleCancel()" >
      <ng-container *nzModalContent>
          <nz-form-item>
            <nz-form-label [nzSpan]="5">Название</nz-form-label>
            <nz-form-control nzHasFeedback [nzSpan]="12" nzErrorTip="Input is required">
              <input nz-input [(ngModel)]="name" required/>
            </nz-form-control>
          </nz-form-item>
          <nz-form-item>
            <nz-form-label [nzSpan]="5">Разделитель</nz-form-label>
            <nz-form-control nzHasFeedback [nzSpan]="12" nzErrorTip="Должен быть один символ">
              <input nz-input [(ngModel)]="separator" maxlength="1" required/>
            </nz-form-control>
          </nz-form-item>
          <nz-form-item>
            <nz-form-label [nzSpan]="5">С хедером</nz-form-label>
            <nz-form-control [nzSpan]="12">
              <label [(ngModel)]="withHeader" nz-checkbox></label>
            </nz-form-control>
          </nz-form-item>
          <nz-form-item>
            <nz-form-label [nzSpan]="5">Файл</nz-form-label>
            <nz-form-control [nzSpan]="12">
              <nz-upload [nzFileList]="fileList" [nzBeforeUpload]="beforeUpload">
                <button nz-button>
                  <nz-icon nzType="upload" />
                  Upload
                </button>
              </nz-upload>
            </nz-form-control>
          </nz-form-item>
      </ng-container>
    </nz-modal>
  `,
})
export class CreateDataConnectorModalComponent {

  private http = inject(HttpClient);
  private connectors = inject(DataConnectorsService);

  public isVisible = model<boolean>(false);

  public connectorCreated = output<DataConnectorListItem>()

  public name = '';
  public separator = ',';
  public withHeader = true;
  public fileList:NzUploadFile[] = [];
  public uploading = false;

  handleCancel() {
    this.isVisible.set(false);
  }

  handleOk() {
    this.handleUpload();
  }

  beforeUpload = (file: NzUploadFile): boolean => {
    this.fileList = [file];
    return false;
  };

  handleUpload(): void {
    const formData = new FormData();
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    this.fileList.forEach((file: any) => {
      formData.append('file', file);
    });
    // You can use any AJAX library you like
    const req = new HttpRequest('POST', `api/data-connectors?name=${encodeURIComponent(this.name)}&separator=${encodeURIComponent(this.separator)}&withHeader=${this.withHeader}`, formData, {
    });
    this.http
      .request(req)
      .pipe(filter(e => e instanceof HttpResponse))
      .subscribe({
        next: () => {
          this.connectors.refresh();
          this.uploading = false;
          this.fileList = [];
          this.isVisible.set(false);
        },
        error: () => {
          this.uploading = false;
        }
      });
  }
}
