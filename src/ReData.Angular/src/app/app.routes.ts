import { Routes } from '@angular/router';
import {DatasetsPage} from '../pages/datasets-page.component';
import {DatasetEditPage} from '../pages/dataset-edit-page.component';
import {HomePageComponent} from '../pages/home-page.component';
import {FunctionsPageComponent} from '../pages/function-page.component';
import {InstructionPageComponent} from '../pages/instruction-page.component';

export const routes: Routes = [
  {
    path: 'functions',
    loadComponent: () => import('../pages/function-page.component').then(m => m.FunctionsPageComponent),
  },
  {
    path: 'docs',
    loadComponent: () => import('../pages/instruction-page.component').then(m => m.InstructionPageComponent),
  },
  {
    path: 'ace',
    loadComponent: () => import('../pages/ace-page.component').then(m => m.AcePageComponent),
  },
  {
    path: 'datasets',
    children: [
      {
        path:"",
        loadComponent: () => import('../pages/datasets-page.component').then(m => DatasetsPage),

      },
      {
        path:":id",
        loadComponent: () => import('../pages/dataset-edit-page.component').then(m => DatasetEditPage),
      },
      {
        path:"new",
        loadComponent: () => import('../pages/dataset-edit-page.component').then(m => m.DatasetEditPage),
      }
    ],
  },
  { path: '**', component: HomePageComponent },
];
