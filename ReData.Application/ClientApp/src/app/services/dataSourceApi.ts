import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { IDataSource, INewDataSource } from '../types';

interface UpdateDataSourceParams {
  id: string;
  rest: Partial<IDataSource>;
}

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
    createDataSource: builder.mutation<IDataSource, INewDataSource>({
      query: (payload) => ({
        url: '/datasource',
        method: 'POST',
        body: payload,
      }),
      transformErrorResponse: (response) => {
        const status = response.status;

        if (status === 500) {
          return {
            status,
            message: 'Failed to connect to the server.',
          };
        } else if (status === 503) {
          const message = response.data.errors[0].message;

          return {
            status,
            message: `${message}. Check the Host and Port fields.` || 'Error',
          };
        }

        return response;
      },
      invalidatesTags: ['DataSource'],
    }),
    updateDataSource: builder.mutation<IDataSource, UpdateDataSourceParams>({
      query: ({ id, rest }) => ({
        url: `/datasource/${id}`,
        method: 'PUT',
        body: rest,
      }),
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
