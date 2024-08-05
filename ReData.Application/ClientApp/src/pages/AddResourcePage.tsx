import {
  ActionIcon,
  Button,
  ComboboxData,
  Divider,
  Flex,
  Group,
  PasswordInput,
  Select,
  Stack,
  TextInput,
  Title,
} from '@mantine/core';
import { hasLength, isNotEmpty, useForm } from '@mantine/form';
import { useDocumentTitle } from '@mantine/hooks';
import { IconChevronLeft } from '@tabler/icons-react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import ErrorAlert from '../components/ErrorAlert';
import { createResource, ResourceRequest } from '../services/api';

const data: ComboboxData = [
  {
    label: 'PostgreSQL',
    value: '1',
  },
  {
    label: 'CSV',
    value: '2',
  },
];

const AddResourcePage: React.FC = () => {
  useDocumentTitle('New resource - ReData');

  const [error, setError] = useState<string | null>(null);

  const navigate = useNavigate();
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

      <form
        id="resource-form"
        onSubmit={form.onSubmit((values) => handleSubmit(values))}
      >
        <Stack gap="0.5em" mb="md">
          <Title order={3} size="1em" fw={500}>
            General
          </Title>

          <TextInput
            label="Name"
            placeholder='i.e. "UsersDB (readonly)" or "Internal Admin API"'
            withAsterisk
            key={form.key('name')}
            {...form.getInputProps('name')}
          />
          <TextInput
            label="Description"
            placeholder="Enter a short description for this resource"
            key={form.key('description')}
            {...form.getInputProps('description')}
          />
          <Select
            label="Resource type"
            placeholder="Start typing to search"
            data={data}
            searchable
            withAsterisk
            key={form.key('type')}
            {...form.getInputProps('type')}
          />
        </Stack>

        <Divider mb="md" />

        <Stack gap="0.5em" mb="md">
          <Title order={3} size="1em" fw={500}>
            Credentials
          </Title>

          <Group align="flex-start">
            <TextInput
              label="Host"
              flex="1"
              withAsterisk
              key={form.key('parameters.host')}
              {...form.getInputProps('parameters.host')}
            />
            <TextInput
              label="Port"
              placeholder="5432"
              withAsterisk
              key={form.key('parameters.port')}
              {...form.getInputProps('parameters.port')}
            />
          </Group>

          <TextInput
            label="Database name"
            placeholder="postgres"
            key={form.key('parameters.database')}
            {...form.getInputProps('parameters.database')}
          />

          <TextInput
            label="Database username"
            placeholder="postgres"
            key={form.key('parameters.username')}
            {...form.getInputProps('parameters.username')}
          />
          <PasswordInput
            label="Database password"
            placeholder="••••••••"
            key={form.key('parameters.password')}
            {...form.getInputProps('parameters.password')}
          />
        </Stack>
      </form>
    </>
  );
};

export default AddResourcePage;
