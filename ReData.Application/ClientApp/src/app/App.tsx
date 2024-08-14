import { MantineProvider } from '@mantine/core';
import '@mantine/core/styles.css';
import { ModalsProvider } from '@mantine/modals';
import { Notifications } from '@mantine/notifications';
import '@mantine/notifications/styles.css';
import { Provider } from 'react-redux';
import { RouterProvider } from 'react-router-dom';
import routes from './routes';
import { store } from './store';

const App: React.FC = () => {
  return (
    <Provider store={store}>
      <MantineProvider defaultColorScheme="auto">
        <ModalsProvider>
          <Notifications />
          <RouterProvider router={routes} />
        </ModalsProvider>
      </MantineProvider>
    </Provider>
  );
};

export default App;
