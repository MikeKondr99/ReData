import { createBrowserRouter } from 'react-router-dom';
import Layout from '../components/Layout';
import AddResourcePage from '../pages/AddResourcePage';
import EditResourcePage from '../pages/EditResourcePage';
import HomePage from '../pages/HomePage';
import ResourcesPage from '../pages/ResourcesPage';

const routes = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: 'resources',
        element: <ResourcesPage />,
      },
      {
        path: 'resources/new',
        element: <AddResourcePage />,
      },
      {
        path: 'resources/:resourceId',
        element: <EditResourcePage />,
      },
    ],
  },
]);

export default routes;
