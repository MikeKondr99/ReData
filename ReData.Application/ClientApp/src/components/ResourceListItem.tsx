import { Box, Button, Card, Flex, Grid, Image, Text } from '@mantine/core';
import { modals } from '@mantine/modals';
import { useNavigate } from 'react-router-dom';
import { IDataSource } from '../app/types';
import { getIcon } from '../utils/iconUtils';

interface ResourceListItemProps {
  resource: IDataSource;
  remove: (id: string) => void;
}

const ResourceListItem: React.FC<ResourceListItemProps> = ({
  resource,
  remove,
}) => {
  const navigate = useNavigate();

  const openDeleteModal = () =>
    modals.openConfirmModal({
      title: 'Delete resource',
      children: (
        <Text size="sm">
          Are you sure you want to delete the resource?
          <br />
          This action cannot be undone.
        </Text>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: () => remove(resource.id),
    });

  return (
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
              <Text>{resource.name}</Text>
              <Text size="sm" color="dimmed">
                {resource.description}
              </Text>
            </Box>
          </Flex>
          <Button.Group>
            <Button onClick={() => navigate(`/datasource/${resource.id}`)}>
              Edit
            </Button>
            <Button onClick={openDeleteModal} variant="filled" color="red">
              Delete
            </Button>
          </Button.Group>
        </Flex>
      </Card>
    </Grid.Col>
  );
};

export default ResourceListItem;
