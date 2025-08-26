import { Routes } from '@angular/router';
import {DatasetsPage} from '../pages/datasets-page.component';
import {DatasetEditPage} from '../pages/dataset-edit-page.component';
import {HomePageComponent} from '../pages/home-page.component';
import {FunctionsPageComponent} from '../pages/function-page.component';
import {InstructionPageComponent} from '../pages/instruction-page.component';

export const routes: Routes = [
  {
    path: 'functions',
    component: FunctionsPageComponent,
  },
  {
    path: 'docs',
    component: InstructionPageComponent,
  },
  {
    path: 'datasets',
    children: [
      {
        path:"",
        component: DatasetsPage

      },
      {
        path:":id",
        component: DatasetEditPage
      },
      {
        path:"new",
        component: DatasetEditPage
      }
    ],
  },
  { path: '**', component: HomePageComponent },
];
