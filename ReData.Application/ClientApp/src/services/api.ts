export interface ResourceRequest {
  name: string;
  description: string;
  type: number;
  parameters: {
    host: string;
    port: string;
    database: string;
    username: string;
    password: string;
  };
}

export const getAllResources = async () => {
  try {
    const response = await fetch('/api/datasource');
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
    const data = await response.json();
    return data;
  } catch (err) {
    console.log(err);
    return null;
  }
};

export const createResource = async (resourceObj: ResourceRequest) => {
  try {
    const response = await fetch('/api/datasource', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(resourceObj),
    });
    if (!response.ok) {
      const errorData = await response.json();
      console.log(errorData);

      // TODO: Rewrite error text generation on backend
      const errorName = Object.keys(errorData.errors[0].metadata)[0];
      const errorMessage = errorData.errors[0].metadata[errorName];

      throw new Error(
        `${errorName} ${errorMessage}. You already have a resource named "${resourceObj.name}"` ||
          'Network response was not ok',
      );
    }
    const data = await response.json();
    return data;
  } catch (err) {
    console.log(err);
    throw err;
  }
};

export const deleteResource = async (id: string) => {
  try {
    const response = await fetch(`/api/datasource/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Network response was not ok');
    }
    return true;
  } catch (err) {
    console.log(err);
    throw err;
  }
};
