import { ActionIcon, Button, Divider, Flex, Group, Title } from '@mantine/core';
import { useDocumentTitle } from '@mantine/hooks';
import { IconChevronLeft } from '@tabler/icons-react';
import { useNavigate } from 'react-router-dom';
import DataSourceForm from '../components/NewDataSourceForm';

const NewAddDataSourcePage: React.FC = () => {
  useDocumentTitle('New data source - ReData');
  const navigate = useNavigate();

  return (
    <>
      <Flex justify="space-between" align="center" mt="lg" mb="lg">
        <Group align="baseline" gap="0.5em">
          <ActionIcon
            onClick={() => navigate('..', { relative: 'path' })}
            variant="subtle"
            color="gray"
            size="sm"
          >
            <IconChevronLeft
              style={{ width: '80%', height: '80%' }}
              stroke={2}
            />
          </ActionIcon>
          <Title order={2}>Test config</Title>
        </Group>

        <Group align="center">
          <Button type="submit" size="compact-sm" form="datasource-form" disabled>
            Create data source
          </Button>
        </Group>
      </Flex>

      <Divider mb="sm" />

      <DataSourceForm />
    </>
  );
};

export default NewAddDataSourcePage;
