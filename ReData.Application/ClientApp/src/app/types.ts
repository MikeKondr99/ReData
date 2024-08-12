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
