import { Button, Flex, Group, Title } from '@mantine/core';
import { useNavigate } from 'react-router-dom';
import DataSourceList from '../components/DataSourceList';

const DataSourcesPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <>
      <Flex align="center" justify="space-between" mt="lg" mb="lg">
        <Title order={2}>Data sources</Title>

        <Group>
          <Button onClick={() => navigate('/datasource/new')} size="compact-sm">
            Add new data source
          </Button>
        </Group>
      </Flex>

      <DataSourceList />
    </>
  );
};

export default DataSourcesPage;
