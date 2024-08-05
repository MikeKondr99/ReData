import { MantineProvider } from '@mantine/core';
import '@mantine/core/styles.css';
import { RouterProvider } from 'react-router-dom';

import { router } from './router';

const App: React.FC = () => {
  return (
    <MantineProvider defaultColorScheme="auto">
      <RouterProvider router={router} />
    </MantineProvider>
  );
};

export default App;
