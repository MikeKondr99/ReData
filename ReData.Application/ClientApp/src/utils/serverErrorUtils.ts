import { ServerError } from '../app/types';

export const extractErrorMessages = (errorResponse: ServerError): string => {
  if (!errorResponse.errors || errorResponse.errors.length === 0) {
    return 'No errors found.';
  }

  const messages: string[] = [];

  errorResponse.errors.forEach((error) => {
    messages.push(error.message);

    for (const key in error.metadata) {
      const metadataValue = error.metadata[key];
      if (typeof metadataValue === 'string') {
        messages.push(`${key}: ${metadataValue}`);
      } else if (Array.isArray(metadataValue)) {
        messages.push(`${key}: ${metadataValue.join(', ')}`);
      }
    }
  });

  return messages.join('\n');
};
