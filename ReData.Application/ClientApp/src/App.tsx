import { MantineProvider } from '@mantine/core';
import { ModalsProvider } from '@mantine/modals';
import '@mantine/core/styles.css';
import { RouterProvider } from 'react-router-dom';

import { router } from './router';

const App: React.FC = () => {
  return (
    <MantineProvider defaultColorScheme="auto">
      <ModalsProvider>
        <RouterProvider router={router} />
      </ModalsProvider>
    </MantineProvider>
  );
};

export default App;
