import { Routes } from '@angular/router';
import {DatasetsPage} from '../pages/datasets-page.component';
import {DatasetEditPage} from '../pages/dataset-edit-page.component';
import {HomePageComponent} from '../pages/home-page.component';

export const routes: Routes = [
  {
    path: 'functions',
    loadComponent: () => import('../pages/function-page.component').then(m => m.FunctionsPageComponent),
  },
  {
    path: 'docs/:path',
    loadComponent: () => import('../components/documentation-viewer.component').then(m => m.DocumentationViewerComponent),
  },
  {
    path: 'docs',
    loadComponent: () => import('../components/documentation-viewer.component').then(m => m.DocumentationViewerComponent),
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
