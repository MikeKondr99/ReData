import { Box, Text } from '@mantine/core';

interface ErrorAlertProps {
  message: string;
}

const ErrorAlert: React.FC<ErrorAlertProps> = (props) => {
  const { message } = props;

  return (
    <Box
      bg="rgba(255, 92, 92, 1)"
      p="1em"
      style={{
        borderRadius: '0.5em',
      }}
    >
      <Text c="white" fw="bold">{message}</Text>
    </Box>
  );
};

export default ErrorAlert;
