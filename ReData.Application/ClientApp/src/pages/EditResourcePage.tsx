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
import { useEffect } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { dataSourceApi } from '../app/services/dataSourceApi';
import ResourceForm from '../components/ResourceForm';
import ErrorAlert from '../components/ErrorAlert';

const EditResourcePage: React.FC = () => {
  useDocumentTitle('Edit resource - ReData');
  const { resourceId } = useParams();
  const navigate = useNavigate();

  const { data, error, isLoading, isFetching } =
    dataSourceApi.useGetDataSourceByIdQuery(resourceId);
  const [updateDataSource, { isLoading: isLoadingUpdate, error: errorUpdate }] =
    dataSourceApi.useUpdateDataSourceMutation();

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
    if (data) {
      form.setValues({
        name: data.name || '',
        description: data.description || '',
        type: data.type || '',
        parameters: {
          host: data.parameters?.host || '',
          port: data.parameters?.port || '5432',
          database: data.parameters?.database || '',
          username: data.parameters?.username || '',
          password: data.parameters?.password || '',
        },
      });
    }
  }, [data]);

  const handleSubmit = async (values: typeof form.values) => {
    try {
      if (!resourceId) {
        throw new Error('Resource ID is required');
      }

      await updateDataSource({ id: resourceId, rest: values }).unwrap();

      navigate('/datasource');
    } catch (error) {
      console.error('Error:', error);
    }
  };

  if (error && error.status === 404)
    return (
      <Text>
        Not found. <Link to="/datasource">Back to Data Source list</Link>
      </Text>
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
          {isLoadingUpdate && <Text>sending</Text>}

          <Button type="submit" size="compact-sm" form="resource-form">
            Update resource
          </Button>
        </Group>
      </Flex>

      <Divider mb="sm" />

      {errorUpdate && <ErrorAlert message={errorUpdate.message} />}

      {isLoading && isFetching ? (
        <Box>
          <Loader />
          <Text>Loading...</Text>
        </Box>
      ) : (
        <ResourceForm isEditing form={form} onSubmit={handleSubmit} />
      )}
    </>
  );
};

export default EditResourcePage;
