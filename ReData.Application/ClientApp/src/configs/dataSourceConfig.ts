export interface DataSourceConfigField {
  name: string;
  label: string;
  type: string;
  placeholder?: string;
  required?: boolean;
  recommended?: boolean;
  defaultValue?: string | number | null;
  mediaType?: string;
}

export type DataSourceType = 'PostgreSQL' | 'CSV' | 'MongoDB';

export interface DataSourceFormValues {
  name: string;
  description: string;
  type: DataSourceType | null;
  parameters: DataSourceParameters;
}

export interface DataSourceParameters {
  [key: string]: string | number | null;
}

const dataSourceConfig: Record<DataSourceType, DataSourceConfigField[]> = {
  PostgreSQL: [
    {
      name: 'host',
      label: 'Host',
      type: 'text',
      required: true,
    },
    {
      name: 'port',
      label: 'Port',
      type: 'number',
      required: true,
      defaultValue: 5432,
      placeholder: '5432',
    },
    {
      name: 'database',
      label: 'Database',
      type: 'text',
      placeholder: 'postgres',
    },
    {
      name: 'username',
      label: 'Username',
      type: 'text',
      placeholder: 'postgres',
    },
    {
      name: 'password',
      label: 'Password',
      type: 'password',
      placeholder: 'postgres',
    },
  ],
  CSV: [
    {
      name: 'file',
      label: 'File',
      type: 'file',
      required: true,
      mediaType: 'text/csv',
    },
  ],
  MongoDB: [
    { name: 'host', label: 'Host', type: 'text', required: true },
    {
      name: 'port',
      label: 'Port',
      type: 'number',
      required: true,
      defaultValue: 27017,
    },
    { name: 'user', label: 'Username', type: 'text', required: true },
    { name: 'password', label: 'Password', type: 'password', required: true },
  ],
};

export default dataSourceConfig;
