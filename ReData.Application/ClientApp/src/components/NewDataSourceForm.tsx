import { Button, Flex, Select, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';

interface Field {
  name: string;
  label: string;
  type: string;
  placeholder?: string;
  required?: boolean;
  recommended?: boolean;
  defaultValue?: string | number;
}

type DataSourceType = 'PostgreSQL' | 'CSV' | 'MongoDB';

const dataSourceFields: Record<DataSourceType, Field[]> = {
  PostgreSQL: [
    { name: 'host', label: 'Host', type: 'text', required: true },
    {
      name: 'port',
      label: 'Port',
      type: 'number',
      required: true,
      defaultValue: 5432,
    },
    { name: 'database', label: 'Database', type: 'text', recommended: true },
    { name: 'user', label: 'Username', type: 'text', recommended: true },
    {
      name: 'password',
      label: 'Password',
      type: 'password',
      recommended: true,
    },
  ],
  CSV: [{ name: 'file', label: 'File', type: 'file', required: true }],
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
  [key: string]: string | number;
}

const DataSourceForm = () => {
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

  const handleTypeChange = (value: string | null) => {
    form.setFieldValue('type', value as DataSourceType);

    if (value && value in dataSourceFields) {
      const fields = dataSourceFields[value as DataSourceType];
      const newParameters: Parameters = {};

      if (fields) {
        fields.forEach((field) => {
          newParameters[field.name] = field.defaultValue || '';
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

    console.log(form.key);
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <Select
        {...form.getInputProps('type')}
        label="Data source type"
        placeholder="Start typing to search"
        searchable
        data={Object.keys(dataSourceFields)}
        onChange={handleTypeChange}
      />

      <Title order={3}>Rendered inputs</Title>

      <Flex direction={'column'}>
        {form.getValues().type &&
          dataSourceFields[form.getValues().type].map((field) => (
            <TextInput
              {...form.getInputProps(`parameters.${field.name}`)}
              key={`${form.getValues().type}-${field.name}`}
              label={field.label}
            />
          ))}
      </Flex>

      <Button type="submit">Submit</Button>
    </form>
  );
};

export default DataSourceForm;
