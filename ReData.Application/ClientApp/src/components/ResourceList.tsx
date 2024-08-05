import { Grid, Loader, Stack, Text } from '@mantine/core';
import React, { useEffect, useState } from 'react';
import { useDocumentTitle } from '@mantine/hooks';
import { getAllResources, deleteResource } from '../services/api';
import ResourceListItem, { ResourceItem } from './ResourceListItem';

const ResourceList: React.FC = () => {
  useDocumentTitle('Resources - ReData');

  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<boolean>(false);
  const [resources, setResources] = useState<ResourceItem[]>([]);

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
  }, []);

  const handleDeleteResource = async (id: string) => {
    try {
      await deleteResource(id);
      setResources((prevResources) =>
        prevResources.filter((resource) => resource.id !== id),
      );
    } catch (err) {
      console.error('Error deleting resource:', err);
    }
  };

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
      {resources.length === 0 ? (
        <Text>No resources found</Text>
      ) : (
        resources.map((resource) => (
          <ResourceListItem
            key={resource.id}
            resource={resource}
            onDelete={handleDeleteResource}
          />
        ))
      )}
    </Grid>
  );
};

export default ResourceList;
