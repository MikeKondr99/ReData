import { ReactElement } from 'react';
import ResourceForm from './components/ResourceForm';
import ResourceList from './components/ResourceList';
import HomePage from './pages/HomePage';
import EditResource from './components/EditResource';

interface Route {
  index?: boolean;
  path?: string;
  element: ReactElement;
}

const AppRoutes: Route[] = [
  {
    index: true,
    element: <HomePage />,
  },
  {
    path: '/new',
    element: <ResourceList />,
  },
  {
    path: '/new/:resourceType',
    element: <ResourceForm />,
  },
  {
    path: '/edit/:resourceType/:resourceId',
    element: <EditResource />,
  },
];

export default AppRoutes;
