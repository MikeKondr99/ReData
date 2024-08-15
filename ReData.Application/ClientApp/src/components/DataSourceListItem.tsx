import { Box, Button, Card, Flex, Grid, Image, Text } from '@mantine/core';
import { modals } from '@mantine/modals';
import { useNavigate } from 'react-router-dom';
import { IDataSource } from '../app/types';
import { getIcon } from '../utils/iconUtils';

interface DataSourceListItemProps {
  datasource: IDataSource;
  remove: (id: string) => void;
}

const DataSourceListItem: React.FC<DataSourceListItemProps> = ({
  datasource,
  remove,
}) => {
  const navigate = useNavigate();

  const openDeleteModal = () =>
    modals.openConfirmModal({
      title: 'Delete data source',
      children: (
        <Text size="sm">
          Are you sure you want to delete this data source?
          <br />
          This action cannot be undone.
        </Text>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: () => remove(datasource.id),
    });

  return (
    <Grid.Col key={datasource.id}>
      <Card shadow="sm" padding="lg">
        <Flex align="center" justify="space-between">
          <Flex align="center">
            <Image
              src={getIcon(datasource.type)}
              width={40}
              height={40}
              alt={datasource.type}
            />
            <Box>
              <Text>{datasource.name}</Text>
              <Text size="sm" color="dimmed">
                {datasource.description}
              </Text>
            </Box>
          </Flex>
          <Button.Group>
            <Button onClick={() => navigate(`/datasource/${datasource.id}`)}>
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

export default DataSourceListItem;
