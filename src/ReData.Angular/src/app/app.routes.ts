import { Routes } from '@angular/router';
import {HomePageComponent} from '../pages/home-page.component';

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
    path: 'datasets',
    children: [
      {
        path:"",
        loadComponent: () => import('../pages/datasets-page.component').then(m => m.DatasetsPage),
      },
      {
        path:":id",
        loadComponent: () => import('../pages/dataset-edit-page.component').then(m => m.DatasetEditPage),
      },
      {
        path:"new",
        loadComponent: () => import('../pages/dataset-edit-page.component').then(m => m.DatasetEditPage),
      }
    ],
  },
  { path: '**', component: HomePageComponent },
];
