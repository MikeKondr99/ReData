import { createBrowserRouter } from 'react-router-dom';
import Layout from '../components/Layout';
import AddDataSourcePage from '../pages/AddDataSourcePage';
import EditDataSourcePage from '../pages/EditDataSourcePage';
import HomePage from '../pages/HomePage';
import DataSourcesPage from '../pages/DataSourcesPage';

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
        path: 'datasource',
        element: <DataSourcesPage />,
      },
      {
        path: 'datasource/new',
        element: <AddDataSourcePage />,
      },
      {
        path: 'datasource/:datasourceId',
        element: <EditDataSourcePage />,
      },
    ],
  },
]);

export default routes;
