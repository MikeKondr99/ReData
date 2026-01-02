import { Routes } from '@angular/router';
import {HomePageComponent} from '../pages/home-page.component';
import {DocumentationViewerComponent} from '../components/documentation-viewer.component';

export const routes: Routes = [
  {
    path: 'functions',
    loadComponent: () => import('../pages/function-page.component').then(m => m.FunctionsPageComponent),
  },
  {
    path: 'docs',
    children: [
      {
        path: '**',
        loadComponent: () => import('../components/documentation-viewer.component').then(m => m.DocumentationViewerComponent),
      },
      {
        path: '',
        loadComponent: () => import('../components/documentation-viewer.component').then(m => m.DocumentationViewerComponent),
      },
    ]
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
