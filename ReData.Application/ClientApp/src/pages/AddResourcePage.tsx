import { ActionIcon, Button, Divider, Flex, Group, Title } from '@mantine/core';
import { hasLength, isNotEmpty, useForm } from '@mantine/form';
import { useDocumentTitle } from '@mantine/hooks';
import { IconChevronLeft } from '@tabler/icons-react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import ErrorAlert from '../components/ErrorAlert';
import ResourceForm from '../components/ResourceForm';
import { createResource, ResourceRequest } from '../services/api';

const AddResourcePage: React.FC = () => {
  useDocumentTitle('New resource - ReData');
  const navigate = useNavigate();

  const [error, setError] = useState<string | null>(null);

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
      setError(null); // Clear previous errors
      const resourceObj: ResourceRequest = {
        ...values,
        type: parseInt(values.type, 10),
      };
      const result = await createResource(resourceObj);
      if (result) {
        console.log('Resource created successfully:', result);
        navigate('/resources');
      } else {
        console.error('Failed to create resource');
      }
      // TODO: Do something about error types
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (err: any) {
      setError(err.message || 'An unexpected error occurred');
      console.error('Error creating resource:', err);
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

      {error && <ErrorAlert message={error} />}

      <ResourceForm form={form} onSubmit={handleSubmit} />
    </>
  );
};

export default AddResourcePage;
