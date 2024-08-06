import { Alert } from '@mantine/core';
import { IconExclamationCircle } from '@tabler/icons-react';

interface ErrorAlertProps {
  message: string;
}

const ErrorAlert: React.FC<ErrorAlertProps> = (props) => {
  const { message } = props;
  const icon = <IconExclamationCircle />;

  return (
    <Alert variant="light" color="red" title="Error" icon={icon}>
      {message}
    </Alert>
  );
};

export default ErrorAlert;
