import React from 'react';
import { Container, Flex, Image, Text, Card, Grid, Button } from '@mantine/core';
import { useResource } from '../ResourceContext';
import { Link } from 'react-router-dom';

import postgresqlIcon from '../assets/postgres.png';
import mysqlIcon from '../assets/mysql.svg';
import mongodbIcon from '../assets/mongodb.png';

const getIcon = (type: string) => {
  switch (type) {
    case 'postgresql':
      return postgresqlIcon;
    case 'mysql':
      return mysqlIcon;
    case 'mongodb':
      return mongodbIcon;
    default:
      return '';
  }
};

const AddedResourceList: React.FC = () => {
  const { resources } = useResource();

  return (
    <Container>
      <Grid>
        {resources.map((resource, index) => (
          <Grid.Col key={index}>
            <Card shadow="sm" padding="lg">
              <Flex align="center" justify="space-between">
                <Flex align="center">
                  <Image src={getIcon(resource.type)} width={40} height={40} alt={resource.type} />
                  <div>
                    <Text w={500}>{resource.name}</Text>
                    <Text size="sm" color="dimmed">{resource.description}</Text>
                  </div>
                </Flex>
                <Link to={`/edit/${resource.type}/${index}`}>
                  <Button>Edit</Button>
                </Link>
              </Flex>
            </Card>
          </Grid.Col>
        ))}
      </Grid>
    </Container>
  );
};

export default AddedResourceList;
