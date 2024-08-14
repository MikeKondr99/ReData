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
import { notifications } from '@mantine/notifications';
import {
  IconCheck,
  IconChevronLeft,
  IconExclamationMark,
} from '@tabler/icons-react';
import { useEffect } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { dataSourceApi } from '../app/services/dataSourceApi';
import { ClientErrorResponse } from '../app/types';
import ErrorAlert from '../components/ErrorAlert';
import DataSourceForm from '../components/DataSourceForm';

const EditDataSourcePage: React.FC = () => {
  useDocumentTitle('Edit data source - ReData');
  const { datasourceId } = useParams();
  const navigate = useNavigate();

  const {
    data: datasource,
    error: datasourceError,
    isLoading: isDatasourceLoading,
    isFetching: isDatasourceFetching,
  } = dataSourceApi.useGetDataSourceByIdQuery(datasourceId as string);
  const [
    updateDataSource,
    {
      isLoading: isUpdateLoading,
      error: errorUpdate,
      isSuccess: isUpdateSuccess,
      isError: isUpdateError,
    },
  ] = dataSourceApi.useUpdateDataSourceMutation();

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
      type: isNotEmpty('You need to select the type of data source'),
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
    if (datasource) {
      form.setValues({
        name: datasource.name || '',
        description: datasource.description || '',
        type: datasource.type || '',
        parameters: {
          host: datasource.parameters?.host || '',
          port: datasource.parameters?.port || '5432',
          database: datasource.parameters?.database || '',
          username: datasource.parameters?.username || '',
          password: datasource.parameters?.password || '',
        },
      });
    }
  }, [datasource]);

  const handleSubmit = async (values: typeof form.values) => {
    try {
      if (!datasourceId) {
        throw new Error('Data source ID is required');
      }

      await updateDataSource({ id: datasourceId, rest: values }).unwrap();

      setTimeout(() => {
        notifications.update({
          id: 'datasource',
          title: 'Data source updated successfully',
          message: 'Your changes have been saved.',
          withCloseButton: true,
          position: 'top-right',
          color: 'green',
          icon: <IconCheck />,
          loading: false,
          autoClose: 3000,
        });

        navigate('/datasource');
      }, 1000);
    } catch (error) {
      console.error('Error:', error);
    }
  };

  useEffect(() => {
    if (isUpdateLoading) {
      notifications.show({
        id: 'datasource',
        title: 'Updating data source',
        message: 'Please wait until the request is processed.',
        withCloseButton: false,
        position: 'top-right',
        loading: true,
        autoClose: false,
      });
    }

    if (isUpdateError) {
      notifications.update({
        id: 'datasource',
        title: 'Error updating data source',
        message: 'Please check the details and try again.',
        withCloseButton: true,
        position: 'top-right',
        color: 'red',
        icon: <IconExclamationMark />,
        loading: false,
        autoClose: 3000,
      });
    }
  }, [isUpdateLoading, isUpdateError, isUpdateSuccess]);

  if (
    datasourceError &&
    'status' in datasourceError &&
    datasourceError.status === 404
  )
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
          <Title order={2}>Update data source</Title>
        </Group>

        <Group align="center">
          <Button
            type="submit"
            size="compact-sm"
            form="datasource-form"
            disabled={isUpdateLoading || isUpdateSuccess}
          >
            Update data source
          </Button>
        </Group>
      </Flex>

      <Divider mb="sm" />

      {errorUpdate && (
        <ErrorAlert mb={'1em'} error={errorUpdate as ClientErrorResponse} />
      )}

      {isDatasourceLoading && isDatasourceFetching ? (
        <Box>
          <Loader />
          <Text>Loading...</Text>
        </Box>
      ) : (
        <DataSourceForm isEditing form={form} onSubmit={handleSubmit} />
      )}
    </>
  );
};

export default EditDataSourcePage;
