import { Divider, Stack, Title } from '@mantine/core';

interface FieldsBlockProps {
  label: string;
  dividerBefore?: boolean;
  // dividerAfter?: boolean;
  children?: React.ReactNode;
}

const FieldsBlock: React.FC<FieldsBlockProps> = (props) => {
  const { children, label, dividerBefore } = props;

  return (
    <>
      {dividerBefore && <Divider mb="md" />}

      <Stack gap="0.5em" mb="md">
        <Title order={3} size="1em" fw={500}>
          {label}
        </Title>

        {children}
      </Stack>
    </>
  );
};

export default FieldsBlock;
