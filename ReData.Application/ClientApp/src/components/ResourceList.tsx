import {
  ActionIcon,
  Button,
  Container,
  Divider,
  Flex,
  Image,
  SimpleGrid,
  Space,
  Title,
} from '@mantine/core';
import { Link } from 'react-router-dom';

import { IconChevronLeft } from '@tabler/icons-react';

import postgresqlIcon from '../assets/postgres.png';
import mysqlIcon from '../assets/mysql.svg';
import mongodbIcon from '../assets/mongodb.png';

const resources = [
  { name: 'PostgreSQL', path: 'postgresql', icon: postgresqlIcon },
  { name: 'MySQL', path: 'mysql', icon: mysqlIcon },
  { name: 'MongoDB', path: 'mongodb', icon: mongodbIcon },
];

const ResourceList = () => {
  return (
    <Container>
      <Space h="md" />
      <Flex align="center" justify="space-between">
        <Flex align="center" gap="sm">
          <Link to="/">
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
          <Title order={2}>Choose resource type</Title>
        </Flex>
      </Flex>

      <Space h="md" />
      <Divider />
      <Space h="md" />

      <SimpleGrid cols={4}>
        {resources.map((resource) => (
          <Link key={resource.path} to={`/new/${resource.path}`}>
            <Button
              variant="outline"
              display="flex"
              fullWidth
              leftSection={<Image height={24} src={resource.icon} />}
            >
              {resource.name}
            </Button>
          </Link>
        ))}
      </SimpleGrid>
    </Container>
  );
};

export default ResourceList;
