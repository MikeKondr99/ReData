import { Routes } from '@angular/router';
import {HomeComponent} from '../components/home.component';
import {DataSourceTableComponent} from '../components/datasources.component';

export const routes: Routes = [
  { path: 'datasources', component: DataSourceTableComponent },
  { path: '**', component: HomeComponent },

];
