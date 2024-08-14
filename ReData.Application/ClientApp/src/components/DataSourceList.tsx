import { Grid, Loader, Text } from '@mantine/core';
import { useDocumentTitle } from '@mantine/hooks';
import React from 'react';
import { dataSourceApi } from '../app/services/dataSourceApi';
import { ClientErrorResponse, IDataSource } from '../app/types';
import ErrorAlert from './ErrorAlert';
import DataSourceListItem from './DataSourceListItem';

const DataSourceList: React.FC = () => {
  useDocumentTitle('Data sources - ReData');

  const [removeDataSource] = dataSourceApi.useDeleteDataSourceMutation();
  const {
    data: datasources = [],
    isLoading,
    isFetching,
    error,
  } = dataSourceApi.useGetAllDataSourcesQuery('');

  return (
    <Grid>
      {isLoading && isFetching && <Loader />}
      {error && <ErrorAlert error={error as ClientErrorResponse} />}

      {!isLoading && datasources.length === 0 ? (
        <Text>No data sources found</Text>
      ) : (
        datasources.map((datasource: IDataSource) => (
          <DataSourceListItem
            key={datasource.id}
            datasource={datasource}
            remove={removeDataSource}
          />
        ))
      )}
    </Grid>
  );
};

export default DataSourceList;
