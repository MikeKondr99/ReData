import { createBrowserRouter } from 'react-router-dom';

import Layout from './layouts/Layout';
import AddResourcePage from './pages/AddResourcePage';
import HomePage from './pages/HomePage';
import ResourcesPage from './pages/ResourcesPage';

export const router = createBrowserRouter([
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
    ],
  },
]);
