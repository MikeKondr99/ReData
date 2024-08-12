export interface IDataSource {
  id: string;
  name: string;
  description: string;
  type: string;
  parameters: {
    host: string;
    port: string;
    database?: string;
    username?: string;
    password?: string;
  };
}

export interface INewDataSource {
  name: string;
  description: string;
  type: string;
  parameters: {
    host: string;
    port: string;
    database?: string;
    username?: string;
    password?: string;
  };
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
}

export interface ClientUnhandledErrorResponse extends ClientErrorResponse {
  error: unknown;
}
