import { Grid, Loader, Text } from '@mantine/core';
import { useDocumentTitle } from '@mantine/hooks';
import React from 'react';
import { dataSourceApi } from '../app/services/dataSourceApi';
import { IDataSource } from '../app/types';
import ErrorAlert from './ErrorAlert';
import ResourceListItem from './ResourceListItem';

const ResourceList: React.FC = () => {
  useDocumentTitle('Resources - ReData');

  const [removeDataSource] = dataSourceApi.useDeleteDataSourceMutation();
  const {
    data: resources = [],
    isLoading,
    isFetching,
    error,
  } = dataSourceApi.useGetAllDataSourcesQuery('');

  return (
    <Grid>
      {isLoading && isFetching && <Loader />}
      {error && <ErrorAlert message="error" />}

      {!isLoading && resources.length === 0 ? (
        <Text>No resources found</Text>
      ) : (
        resources.map((resource: IDataSource) => (
          <ResourceListItem
            key={resource.id}
            resource={resource}
            remove={removeDataSource}
          />
        ))
      )}
    </Grid>
  );
};

export default ResourceList;
