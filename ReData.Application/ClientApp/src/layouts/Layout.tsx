import { Container, Divider } from '@mantine/core';

import { Link, Outlet } from 'react-router-dom';

const Layout: React.FC = () => {
  return (
    <>
      <Container pt="lg" pb="lg">
        <Link
          to="/"
          style={{
            fontSize: 'var(--mantine-h2-font-size)',
            fontWeight: 'var(--mantine-h2-font-weight)',
            lineHeight: 'var(--mantine-h2-line-height)',
            color: 'var(--mantine-color-blue-8)',
            textDecoration: 'none',
          }}
        >
          ReData
        </Link>
      </Container>

      <Divider />

      <main>
        <Container>
          <Outlet />
        </Container>
      </main>
    </>
  );
};

export default Layout;
