import { ActionIcon, Button, Divider, Flex, Group, Title } from '@mantine/core';
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
import DataSourceForm from '../components/DataSourceForm';
import { DataSourceFormValues } from '../configs/dataSourceConfig';

const AddDataSourcePage: React.FC = () => {
  useDocumentTitle('New data source - ReData');
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

  const handleSubmit = async (values: DataSourceFormValues) => {
    try {
      await createDataSource(values).unwrap();

      setTimeout(() => {
        notifications.update({
          id: 'datasource',
          title: 'Data source created successfully',
          message: 'The new data source has been added.',
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
        title: 'Error creating data source',
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
          <Title order={2}>Configure new data source</Title>
        </Group>

        <Group align="center">
          <Button
            type="submit"
            size="compact-sm"
            form="new-datasource-form"
            disabled={isCreateLoading || isCreateSuccess}
          >
            Create data source
          </Button>
        </Group>
      </Flex>

      <Divider mb="sm" />

      {createError && (
        <ErrorAlert mb={'1em'} error={createError as ClientErrorResponse} />
      )}

      <DataSourceForm onSubmit={handleSubmit} />
    </>
  );
};

export default AddDataSourcePage;
