import { Button, Flex, Group, Title } from '@mantine/core';
import { useNavigate } from 'react-router-dom';
import ResourceList from '../components/ResourceList';

const ResourcesPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <>
      <Flex align="center" justify="space-between" mt="lg" mb="lg">
        <Title order={2}>Resources</Title>

        <Group>
          <Button onClick={() => navigate('/resources/new')} size="compact-sm">
            Add new resource
          </Button>
        </Group>
      </Flex>

      <ResourceList />
    </>
  );
};

export default ResourcesPage;
