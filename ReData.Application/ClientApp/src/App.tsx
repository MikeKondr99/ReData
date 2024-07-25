import { MantineProvider } from '@mantine/core';
import '@mantine/core/styles.css';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import AppRoutes from './AppRoutes';
import { ResourceProvider } from './ResourceContext';

const App: React.FC = () => {
  return (
    <MantineProvider>
      <Router>
        <ResourceProvider>
          <Routes>
            {AppRoutes.map((route, index) => {
              const { element, ...rest } = route;
              return <Route key={index} {...rest} element={element} />;
            })}
          </Routes>
        </ResourceProvider>
      </Router>
    </MantineProvider>
  );
};

export default App;
