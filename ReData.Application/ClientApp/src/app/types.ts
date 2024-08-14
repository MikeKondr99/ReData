export interface IDataSource {
  id: string;
  name: string;
  description: string;
  type: string;
  parameters: DataSourceParameters;
}

export interface INewDataSource {
  name: string;
  description: string;
  type: string;
  parameters: DataSourceParameters;
}

interface DataSourceParameters {
  host: string;
  port: string;
  database?: string;
  username?: string;
  password?: string;
}

export interface UpdateDataSourceParams {
  id: string;
  rest: Partial<IDataSource>;
}

interface ErrorDetail {
  message: string;
  metadata: {
    [key: string]: string[];
  };
}

interface ErrorResponse {
  errors: ErrorDetail[];
}

export type ServerError = ErrorResponse;

export interface ClientErrorResponse {
  status: number;
  message: string;
  error?: unknown;
}
