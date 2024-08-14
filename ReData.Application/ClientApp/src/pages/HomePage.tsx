import { Divider, Flex, Stack, Title } from '@mantine/core';
import { useDocumentTitle } from '@mantine/hooks';
import { Link } from 'react-router-dom';

const HomePage: React.FC = () => {
  useDocumentTitle('ReData');

  return (
    <>
      <Flex align="center" justify="space-between" mt="lg" mb="lg">
        <Title order={2}>Home</Title>
      </Flex>

      <Divider mb="sm" />

      <Stack>
        <Link to="/datasource">Data sources</Link>
      </Stack>
    </>
  );
};

export default HomePage;
