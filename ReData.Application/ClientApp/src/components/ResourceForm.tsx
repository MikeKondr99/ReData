import {
  ComboboxData,
  Divider,
  Group,
  PasswordInput,
  Select,
  Stack,
  TextInput,
  Title,
} from '@mantine/core';

const data: ComboboxData = [
  {
    label: 'PostgreSQL',
    value: 'PostgreSql',
  },
  {
    label: 'CSV',
    value: 'Csv',
  },
];

interface ResourceFormProps {
  isEditing?: boolean;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onSubmit: any;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  form: any;
}

const ResourceForm: React.FC<ResourceFormProps> = (props) => {
  const { isEditing = false, form, onSubmit } = props;

  return (
    <form id="resource-form" onSubmit={form.onSubmit(onSubmit)}>
      <Stack gap="0.5em" mb="md">
        <Title order={3} size="1em" fw={500}>
          General
        </Title>

        <TextInput
          label="Name"
          placeholder='i.e. "UsersDB (readonly)" or "Internal Admin API"'
          withAsterisk
          key={form.key('name')}
          {...form.getInputProps('name')}
        />
        <TextInput
          label="Description"
          placeholder="Enter a short description for this resource"
          key={form.key('description')}
          {...form.getInputProps('description')}
        />
        {!isEditing && (
          <Select
            label="Resource type"
            placeholder="Start typing to search"
            data={data}
            searchable
            withAsterisk
            key={form.key('type')}
            {...form.getInputProps('type')}
          />
        )}
      </Stack>

      <Divider mb="md" />

      <Stack gap="0.5em" mb="md">
        <Title order={3} size="1em" fw={500}>
          Credentials
        </Title>

        <Group align="flex-start">
          <TextInput
            label="Host"
            flex="1"
            withAsterisk
            key={form.key('parameters.host')}
            {...form.getInputProps('parameters.host')}
          />
          <TextInput
            label="Port"
            placeholder="5432"
            withAsterisk
            key={form.key('parameters.port')}
            {...form.getInputProps('parameters.port')}
          />
        </Group>

        <TextInput
          label="Database name"
          placeholder="postgres"
          key={form.key('parameters.database')}
          {...form.getInputProps('parameters.database')}
        />

        <TextInput
          label="Database username"
          placeholder="postgres"
          key={form.key('parameters.username')}
          {...form.getInputProps('parameters.username')}
        />
        <PasswordInput
          label="Database password"
          placeholder="••••••••"
          key={form.key('parameters.password')}
          {...form.getInputProps('parameters.password')}
        />
      </Stack>
    </form>
  );
};

export default ResourceForm;
