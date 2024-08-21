export const removeExtraSpaces = (input: string): string => {
  const trimmed = input.trim();
  const result = trimmed.replace(/\s+/g, ' ');
  return result;
};
