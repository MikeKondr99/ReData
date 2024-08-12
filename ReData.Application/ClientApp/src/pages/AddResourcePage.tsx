import {
  ActionIcon,
  Button,
  Divider,
  Flex,
  Group,
  Text,
  Title,
} from '@mantine/core';
import { hasLength, isNotEmpty, useForm } from '@mantine/form';
import { useDocumentTitle } from '@mantine/hooks';
import { IconChevronLeft } from '@tabler/icons-react';
import { useNavigate } from 'react-router-dom';
import { dataSourceApi } from '../app/services/dataSourceApi';
import ErrorAlert from '../components/ErrorAlert';
import ResourceForm from '../components/ResourceForm';

const AddResourcePage: React.FC = () => {
  useDocumentTitle('New resource - ReData');
  const navigate = useNavigate();

  const [createDataSource, { error, isLoading }] =
    dataSourceApi.useCreateDataSourceMutation();

  const form = useForm({
    mode: 'uncontrolled',
    clearInputErrorOnChange: true,
    initialValues: {
      name: '',
      description: '',
      type: '',
      parameters: {
        host: '',
        port: '5432',
        database: '',
        username: '',
        password: '',
      },
    },
    validateInputOnChange: ['parameters.port'],
    validate: {
      name: hasLength({ min: 2 }, 'Name must have at least 2 letters'),
      type: isNotEmpty('You need to select the type of resource'),
      parameters: {
        host: isNotEmpty('This field is required'),
        port: (value) =>
          value.length === 0
            ? 'This field is required'
            : /^\d+$/.test(value)
            ? null
            : 'Port should be a number',
      },
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    try {
      await createDataSource(values).unwrap();

      navigate('/resources');
    } catch (error) {
      console.error('Error:', error);
    }
  };

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
          <Title order={2}>Configure new resource</Title>
        </Group>

        <Group align="center">
          <Button type="submit" size="compact-sm" form="resource-form">
            Create resource
          </Button>
        </Group>
      </Flex>

      <Divider mb="sm" />

      {error && <ErrorAlert message={error.message} />}

      {isLoading && <Text>sending</Text>}

      <ResourceForm form={form} onSubmit={handleSubmit} />
    </>
  );
};

export default AddResourcePage;
