import {
  Box,
  Button,
  Card,
  Flex,
  Grid,
  Image,
  Loader,
  Stack,
  Text,
} from '@mantine/core';
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { useDocumentTitle } from '@mantine/hooks';
import mongodbIcon from '../assets/mongodb.png';
import mysqlIcon from '../assets/mysql.svg';
import postgresqlIcon from '../assets/postgres.png';
import { getAllResources } from '../services/api';

const getIcon = (type: string) => {
  switch (type) {
    case 'PostgreSql':
      return postgresqlIcon;
    case 'mysql':
      return mysqlIcon;
    case 'mongodb':
      return mongodbIcon;
    default:
      return '';
  }
};

export interface Resource {
  id: string;
  type: string;
  name: string;
  description: string;
  parameters: {
    user: string;
    port: string;
    database: string;
    host: string;
    password: string;
  };
}

const ResourceList: React.FC = () => {
  useDocumentTitle('Resources - ReData');
  const navigate = useNavigate();

  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<boolean>(false);
  const [resources, setResources] = useState<Resource[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const result = await getAllResources();
        if (result) {
          setResources(result);
        } else {
          setError(true);
        }
      } catch (err) {
        setError(true);
        console.log(err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
    //   setResources([
    //     {
    //       id: '01dc1634-f71d-41ed-87a7-876e4f8d19d0',
    //       type: 'PostgreSql',
    //       name: 'Public Postgres database',
    //       description: '',
    //       parameters: {
    //         user: 'reader',
    //         port: '5432',
    //         database: 'pfmegrnargs',
    //         host: 'hh-pgsql-public.ebi.ac.uk',
    //         password: 'NWDMCE5xdipIjRrp',
    //       },
    //     },
    //     {
    //       id: '09de3100-22a4-44ce-99b8-7f211a731742',
    //       type: 'PostgreSql',
    //       name: 'MyDataSource',
    //       description: 'This is a test data source',
    //       parameters: {
    //         user: 'testuser',
    //         database: 'testdb',
    //         host: 'localhost',
    //         password: 'testpassword',
    //         port: '5432',
    //       },
    //     },
    //   ]);

    //   setLoading(false);
    // }, 1234);
  }, []);

  if (loading) {
    return (
      <Stack>
        <Text>Loading...</Text>
        <Loader />
      </Stack>
    );
  }

  if (error) {
    return <Text>Error :(</Text>;
  }

  return (
    <Grid>
      {resources.map((resource) => (
        <Grid.Col key={resource.id}>
          <Card shadow="sm" padding="lg">
            <Flex align="center" justify="space-between">
              <Flex align="center">
                <Image
                  src={getIcon(resource.type)}
                  width={40}
                  height={40}
                  alt={resource.type}
                />
                <Box>
                  <Text w={500}>{resource.name}</Text>
                  <Text size="sm" color="dimmed">
                    {resource.description}
                  </Text>
                </Box>
              </Flex>
              <Button.Group>
                <Button
                  onClick={() => navigate(`/resources/${resource.id}/edit`)}
                >
                  Edit
                </Button>
                <Button variant="filled" color="red">
                  Delete
                </Button>
              </Button.Group>
            </Flex>
          </Card>
        </Grid.Col>
      ))}
    </Grid>
  );
};

export default ResourceList;
