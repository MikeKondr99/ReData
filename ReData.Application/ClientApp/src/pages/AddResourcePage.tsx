import { ActionIcon, Button, Divider, Flex, Group, Title } from '@mantine/core';
import { hasLength, isNotEmpty, useForm } from '@mantine/form';
import { useDocumentTitle } from '@mantine/hooks';
import { notifications } from '@mantine/notifications';
import {
  IconCheck,
  IconChevronLeft,
  IconExclamationMark,
} from '@tabler/icons-react';
import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { dataSourceApi } from '../app/services/dataSourceApi';
import { ClientErrorResponse } from '../app/types';
import ErrorAlert from '../components/ErrorAlert';
import ResourceForm from '../components/ResourceForm';

const AddResourcePage: React.FC = () => {
  useDocumentTitle('New resource - ReData');
  const navigate = useNavigate();

  const [
    createDataSource,
    {
      error: createError,
      isLoading: isCreateLoading,
      isError: isCreateError,
      isSuccess: isCreateSuccess,
    },
  ] = dataSourceApi.useCreateDataSourceMutation();

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

      setTimeout(() => {
        notifications.update({
          id: 'datasource',
          title: 'Data source has been created',
          message: 'Congrats.',
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
    if (isCreateLoading) {
      notifications.show({
        id: 'datasource',
        title: 'Creating data source',
        message: 'Wait until the request is processed. Do not close the page.',
        withCloseButton: false,
        position: 'top-right',
        loading: true,
        autoClose: false,
      });
    }

    if (isCreateError) {
      notifications.update({
        id: 'datasource',
        title: 'Error creating the data source',
        message: 'Double-check the entered data and try again.',
        withCloseButton: true,
        position: 'top-right',
        color: 'red',
        icon: <IconExclamationMark />,
        loading: false,
        autoClose: 3000,
      });
    }
  }, [isCreateLoading, isCreateError]);

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
          <Button
            type="submit"
            size="compact-sm"
            form="resource-form"
            disabled={isCreateLoading || isCreateSuccess}
          >
            Create resource
          </Button>
        </Group>
      </Flex>

      <Divider mb="sm" />

      {createError && (
        <ErrorAlert mb={'1em'} error={createError as ClientErrorResponse} />
      )}

      <ResourceForm form={form} onSubmit={handleSubmit} />
    </>
  );
};

export default AddResourcePage;
