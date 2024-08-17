import {
  Box,
  Button,
  FileInput,
  Group,
  MultiSelect,
  PasswordInput,
  Select,
  TextInput,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { IconFile } from '@tabler/icons-react';
import { useState } from 'react';
import dataSourceFields, {
  DataSourceType,
  DataSourceConfigField as Field,
  DataSourceFormValues as FormValues,
  DataSourceParameters as Parameters,
} from '../configs/dataSourceConfig';
import FieldsBlock from './FieldsBlock';

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

  const [selectedParameters, setSelectedParameters] = useState<string[]>([]);
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

      form.setFieldValue('parameters', newParameters);
      setSelectedParameters([]);
    } else {
      form.setFieldValue('type', null);
      form.setFieldValue('parameters', {});
      setSelectedParameters([]);
    }
  };

  const handleRemoveParameter = (parameter: string) => {
    setSelectedParameters((prev) => prev.filter((p) => p !== parameter));
    form.setFieldValue(`parameters.${parameter}`, null);
  };

  const handleSubmit = (values: FormValues) => {
    console.log(values);
  };

  const renderField = (field: Field) => {
    const commonProps = {
      ...form.getInputProps(`parameters.${field.name}`),
      label: field.label,
      required: field.required,
    };

    switch (field.type) {
      case 'number':
      case 'text':
        return (
          <TextInput
            {...commonProps}
            key={`${currentType}-${field.name}`}
            placeholder={field.placeholder}
            flex={1}
          />
        );
      case 'file':
        return (
          <FileInput
            {...commonProps}
            key={`${currentType}-${field.name}`}
            accept={field.mediaType}
            placeholder="CSV-file with data"
            leftSectionPointerEvents="none"
            leftSection={<IconFile />}
            flex={1}
          />
        );
      case 'password':
        return (
          <PasswordInput
            {...commonProps}
            key={`${currentType}-${field.name}`}
            placeholder="••••••••"
            flex={1}
          />
        );
      default:
        return null;
    }
  };

  return (
    <form id="new-datasource-form" onSubmit={form.onSubmit(handleSubmit)}>
      <FieldsBlock label="General">
        <TextInput
          {...form.getInputProps('name')}
          label="Name"
          placeholder='i.e. "UsersDB (readonly)" or "Internal Admin API"'
          required
        />
        <TextInput
          {...form.getInputProps('description')}
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

            return renderField(field);
          })}
        </FieldsBlock>
      )}

      {currentType ? (
        dataSourceFields[currentType].every(
          (field) => field.required,
        ) ? null : (
          <FieldsBlock label="Recommended parameters" dividerBefore>
            <MultiSelect
              label="Select parameters to add"
              placeholder="Select parameters"
              data={dataSourceFields[currentType]
                .filter((field) => !field.required)
                .map((field) => ({ value: field.name, label: field.label }))}
              value={selectedParameters}
              onChange={setSelectedParameters}
              searchable
              clearable
            />

            {dataSourceFields[currentType]
              .filter((field) => selectedParameters.includes(field.name))
              .map((parameter) => (
                <Group key={parameter.name} mt="xs">
                  {renderField(parameter)}
                  <Button
                    variant="outline"
                    color="red"
                    onClick={() => handleRemoveParameter(parameter.name)}
                  >
                    Remove
                  </Button>
                </Group>
              ))}
          </FieldsBlock>
        )
      ) : null}

      <Box style={{ marginBottom: '150px' }} />
    </form>
  );
};

export default DataSourceForm;
