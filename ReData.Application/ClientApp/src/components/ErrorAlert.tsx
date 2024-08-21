import { Alert, AlertProps } from '@mantine/core';
import { IconExclamationCircle } from '@tabler/icons-react';
import { ClientErrorResponse } from '../app/types';

interface ErrorAlertProps extends AlertProps {
  error: ClientErrorResponse;
}

const ErrorAlert: React.FC<ErrorAlertProps> = (props) => {
  const { error } = props;
  const icon = <IconExclamationCircle />;

  return (
    <Alert variant="light" color="red" icon={icon} {...props}>
      {error.message}
    </Alert>
  );
};

export default ErrorAlert;
