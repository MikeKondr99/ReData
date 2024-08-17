import {
  createApi,
  fetchBaseQuery,
  FetchBaseQueryError,
} from '@reduxjs/toolkit/query/react';
import { extractErrorMessages } from '../../utils/serverErrorUtils';
import {
  ClientErrorResponse,
  IDataSource,
  ServerError,
  UpdateDataSourceParams,
} from '../types';
import { removeExtraSpaces } from '../../utils/textUtils';
import { DataSourceFormValues } from '../../configs/dataSourceConfig';

export const dataSourceApi = createApi({
  reducerPath: 'dataSourceApi',
  baseQuery: fetchBaseQuery({ baseUrl: 'https://localhost:5173/api' }),
  tagTypes: ['DataSource'],
  endpoints: (builder) => ({
    getAllDataSources: builder.query<IDataSource[], string>({
      query: () => '/datasource',
      providesTags: ['DataSource'],
    }),
    getDataSourceById: builder.query<IDataSource, string>({
      query: (id) => `/datasource/${id}`,
      providesTags: ['DataSource'],
    }),
    createDataSource: builder.mutation<IDataSource, DataSourceFormValues>({
      query: (payload) => ({
        url: '/datasource',
        method: 'POST',
        body: {
          ...payload,
          name: removeExtraSpaces(payload.name as string),
          description: removeExtraSpaces(payload.description as string),
        },
      }),
      transformErrorResponse: handleTransformErrorResponse,
      invalidatesTags: ['DataSource'],
    }),
    updateDataSource: builder.mutation<IDataSource, UpdateDataSourceParams>({
      query: ({ id, rest }) => ({
        url: `/datasource/${id}`,
        method: 'PUT',
        body: {
          ...rest,
          name: removeExtraSpaces(rest.name as string),
          description: removeExtraSpaces(rest.description as string),
        },
      }),
      transformErrorResponse: handleTransformErrorResponse,
      invalidatesTags: ['DataSource'],
    }),
    deleteDataSource: builder.mutation<IDataSource, string>({
      query: (id) => ({
        url: `datasource/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['DataSource'],
    }),
  }),
});

function handleTransformErrorResponse(
  response: FetchBaseQueryError,
): ClientErrorResponse {
  const status = response.status;
  const data = response.data as ServerError;

  switch (status) {
    case 400: {
      const message = extractErrorMessages(data);

      return {
        status,
        message,
      };
    }
    case 500: {
      return {
        status,
        message: 'Failed to connect to the server.',
      };
    }
    case 503: {
      const message = data.errors[0].message;

      return {
        status,
        message: `${message}. Check the Host and Port fields.`,
      };
    }

    default: {
      return {
        status,
        message: `HTTP ${status}: Unhandled error.`,
        error: data,
      } as ClientErrorResponse;
    }
  }
}
