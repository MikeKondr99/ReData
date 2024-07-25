import { Button, Container, Divider, Flex, Space, Title } from '@mantine/core';
import { Link } from 'react-router-dom';
import AddedResourceList from '../components/AddedResourceList';

const HomePage = () => {
  return (
    <Container>
      <Space h="md" />

      <Flex align="center" justify="space-between">
        <Title order={2}>Resources</Title>

        <Link to="/new">
          <Button size="compact-md">Add resource</Button>
        </Link>
      </Flex>

      <Space h="md" />
      <Divider />
      <Space h="md" />

      <AddedResourceList />
    </Container>
  );
};

export default HomePage;
