import {
  FileInput,
  PasswordInput,
  Select,
  Text,
  TextInput,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { IconFile } from '@tabler/icons-react';
import FieldsBlock from './FieldsBlock';

interface Field {
  name: string;
  label: string;
  type: string;
  placeholder?: string;
  required?: boolean;
  recommended?: boolean;
  defaultValue?: string | number | null;
  mediaType?: string;
}

type DataSourceType = 'PostgreSQL' | 'CSV' | 'MongoDB';

const dataSourceFields: Record<DataSourceType, Field[]> = {
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
    { name: 'user', label: 'Username', type: 'text', placeholder: 'postgres' },
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

interface FormValues {
  name: string;
  description: string;
  type: DataSourceType | null;
  parameters: Parameters;
}

interface Parameters {
  [key: string]: string | number | unknown;
}

const DataSourceForm: React.FC = () => {
  const initialValues = {
    name: '',
    description: '',
    type: null,
    parameters: {},
  };

  const form = useForm<FormValues>({
    mode: 'uncontrolled',
    initialValues,
  });

  const currentType = form.getValues().type;

  const handleTypeChange = (value: string | null) => {
    form.setFieldValue('type', value as DataSourceType);

    if (value && value in dataSourceFields) {
      const fields = dataSourceFields[value as DataSourceType];
      const newParameters: Parameters = {};

      if (fields) {
        fields.forEach((field) => {
          newParameters[field.name] = field.defaultValue || null;
        });
      }

      console.log(dataSourceFields[value as DataSourceType]);

      form.setFieldValue('parameters', newParameters);
    } else {
      form.setFieldValue('type', null);
      form.setFieldValue('parameters', {});
    }
  };

  const handleSubmit = (values: FormValues) => {
    console.log(values);
  };

  return (
    <form id="new-datasource-form" onSubmit={form.onSubmit(handleSubmit)}>
      <FieldsBlock label="General">
        <TextInput
          {...form.getInputProps('name')}
          key={form.key('name')}
          label="Name"
          placeholder='i.e. "UsersDB (readonly)" or "Internal Admin API"'
          required
        />
        <TextInput
          {...form.getInputProps('description')}
          key={form.key('description')}
          label="Description"
          placeholder="Enter a short description for this data source"
        />
        <Select
          {...form.getInputProps('type')}
          label="Data source type"
          placeholder="Start typing to search"
          searchable
          required
          data={Object.keys(dataSourceFields)}
          onChange={handleTypeChange}
        />
      </FieldsBlock>

      {currentType && (
        <FieldsBlock label="Required parameters" dividerBefore>
          {dataSourceFields[currentType].map((field) => {
            if (!field.required) return null;

            switch (field.type) {
              case 'number':
              case 'text': {
                return (
                  <TextInput
                    {...form.getInputProps(`parameters.${field.name}`)}
                    key={`${currentType}-${field.name}`}
                    label={field.label}
                    placeholder={field.placeholder}
                    required={field.required}
                  />
                );
              }
              case 'file': {
                return (
                  <FileInput
                    {...form.getInputProps(`parameters.${field.name}`)}
                    key={`${currentType}-${field.name}`}
                    label={field.label}
                    required={field.required}
                    accept={field.mediaType}
                    placeholder="CSV-file with data"
                    leftSectionPointerEvents="none"
                    leftSection={<IconFile />}
                  />
                );
              }
              case 'password': {
                return (
                  <PasswordInput
                    {...form.getInputProps(`parameters.${field.name}`)}
                    key={`${currentType}-${field.name}`}
                    label={field.label}
                    placeholder="••••••••"
                    required={field.required}
                  />
                );
              }
            }
          })}
        </FieldsBlock>
      )}

      {/* Checking that there are no objects in the array without the required key */}
      {currentType ? (
        dataSourceFields[currentType].every(
          (field) => field.required,
        ) ? null : (
          <FieldsBlock label="Recommended parameters" dividerBefore>
            <Text>Current type: {currentType}</Text>
          </FieldsBlock>
        )
      ) : null}
    </form>
  );
};

export default DataSourceForm;
