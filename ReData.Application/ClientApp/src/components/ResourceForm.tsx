import React, { useState } from 'react';
import {
  ActionIcon,
  Button,
  Container,
  Divider,
  Flex,
  Space,
  TextInput,
  Title,
} from '@mantine/core';
import { IconChevronLeft } from '@tabler/icons-react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import { useResource } from '../ResourceContext';

const ResourceForm: React.FC = () => {
  const { resourceType } = useParams<{ resourceType: 'postgresql' | 'mysql' | 'mongodb' }>();
  const navigate = useNavigate();
  const { addResource } = useResource();
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    host: '',
    port: 5432,
    dbName: '',
    dbUsername: '',
    dbPassword: '',
    type: resourceType || 'postgresql',
  });

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target;
    setFormData((prevData) => ({ ...prevData, [name]: value }));
  };

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    addResource({
      ...formData,
      type: formData.type as 'postgresql' | 'mysql' | 'mongodb',
    });
    navigate('/');
  };

  const handleButtonClick = () => {
    document.getElementById('resource-form')?.dispatchEvent(new Event('submit', { cancelable: true, bubbles: true }));
  };

  return (
    <div>
      <Container>
        <Space h="md" />
        <Flex align="center" justify="space-between">
          <Flex align="center" gap="sm">
            <Link to="/new">
              <ActionIcon
                variant="light"
                color="gray"
                size="sm"
                radius="xl"
                aria-label="Settings"
              >
                <IconChevronLeft
                  style={{ width: '70%', height: '70%' }}
                  stroke={1.5}
                />
              </ActionIcon>
            </Link>
            <Title order={2}>Configure {resourceType}</Title>
          </Flex>

          <Flex gap="xs">
            <Button variant="outline" size="compact-md" color="gray">
              Test connection
            </Button>
            <Button size="compact-md" onClick={handleButtonClick}>
              Create resource
            </Button>
          </Flex>
        </Flex>

        <Space h="md" />
        <Divider />
        <Space h="md" />

        <form id="resource-form" onSubmit={handleSubmit}>
          <TextInput
            label="Name"
            name="name"
            placeholder='i.e. "UsersDB (readonly)" or "Internal Admin API"'
            required
            mb="xs"
            value={formData.name}
            onChange={handleChange}
          />
          <TextInput
            label="Description"
            name="description"
            placeholder="Enter a short description for this resource"
            mb="xs"
            value={formData.description}
            onChange={handleChange}
          />

          <Divider />
          <Space h="md" />

          <TextInput
            label="Host"
            name="host"
            required
            mb="xs"
            value={formData.host}
            onChange={handleChange}
          />
          <TextInput
            label="Port"
            name="port"
            placeholder="5432"
            required
            mb="xs"
            value={formData.port.toString()}
            onChange={handleChange}
          />

          <TextInput
            label="Database name"
            name="dbName"
            placeholder="hn_api_production"
            mb="xs"
            value={formData.dbName}
            onChange={handleChange}
          />

          <Divider />
          <Space h="md" />

          <TextInput
            label="Database username"
            name="dbUsername"
            placeholder="postgres"
            mb="xs"
            value={formData.dbUsername}
            onChange={handleChange}
          />

          <TextInput
            label="Database password"
            name="dbPassword"
            placeholder="********"
            type="password"
            mb="xs"
            value={formData.dbPassword}
            onChange={handleChange}
          />

        </form>
      </Container>
    </div>
  );
};

export default ResourceForm;
