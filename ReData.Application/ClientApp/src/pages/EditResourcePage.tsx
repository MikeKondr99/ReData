import {
  ActionIcon,
  Box,
  Button,
  Divider,
  Flex,
  Group,
  Loader,
  Text,
  Title,
} from '@mantine/core';
import { hasLength, isNotEmpty, useForm } from '@mantine/form';
import { useDocumentTitle } from '@mantine/hooks';
import { IconChevronLeft } from '@tabler/icons-react';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import ErrorAlert from '../components/ErrorAlert';
import ResourceForm from '../components/ResourceForm';
import { getResource, ResourceRequest, updateResource } from '../services/api';

const EditResourcePage: React.FC = () => {
  useDocumentTitle('Edit resource - ReData');
  const { resourceId } = useParams();
  const navigate = useNavigate();

  const [loading, setLoading] = useState<boolean>(true);
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

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);

        const result = await getResource(resourceId);
        form.setValues(result);

        setLoading(false);
      } catch (err) {
        setError('Error fetching data');
        setLoading(false);
      }
    };

    fetchData();
  }, [resourceId]);

  const handleSubmit = async (values: typeof form.values) => {
    try {
      setError(null); // Clear previous errors
      const resourceObj: ResourceRequest = {
        ...values,
        // TODO: Find a solution
        // Kludge because it is created with a number, and returns a string
        type: values.type === 'PostgreSql' ? 1 : parseInt(values.type, 10),
      };
      const result = await updateResource(resourceId, resourceObj);
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

  if (loading)
    return (
      <Box>
        <Loader />
        <Text>Loading...</Text>
      </Box>
    );

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
          <Title order={2}>Update resource</Title>
        </Group>

        <Group align="center">
          <Button type="submit" size="compact-sm" form="resource-form">
            Update resource
          </Button>
        </Group>
      </Flex>

      <Divider mb="sm" />

      {error && <ErrorAlert message={error} />}

      <ResourceForm isEditing form={form} onSubmit={handleSubmit} />
    </>
  );
};

export default EditResourcePage;
