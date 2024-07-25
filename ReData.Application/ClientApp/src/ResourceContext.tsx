import React, { createContext, useState, useContext, ReactNode } from 'react';

interface Resource {
  name: string;
  description: string;
  host: string;
  port: number;
  dbName: string;
  dbUsername: string;
  dbPassword: string;
  type: 'postgresql' | 'mysql' | 'mongodb';
}

interface ResourceContextType {
  resources: Resource[];
  addResource: (resource: Resource) => void;
  updateResource: (index: number, resource: Resource) => void;
}

const ResourceContext = createContext<ResourceContextType | undefined>(undefined);

export const useResource = () => {
  const context = useContext(ResourceContext);
  if (!context) {
    throw new Error('useResource must be used within a ResourceProvider');
  }
  return context;
};

interface ResourceProviderProps {
  children: ReactNode;
}

export const ResourceProvider: React.FC<ResourceProviderProps> = ({ children }) => {
  const [resources, setResources] = useState<Resource[]>([]);

  const addResource = (resource: Resource) => {
    setResources((prevResources) => [...prevResources, resource]);
  };

  const updateResource = (index: number, updatedResource: Resource) => {
    setResources((prevResources) =>
      prevResources.map((resource, i) => (i === index ? updatedResource : resource))
    );
  };

  return (
    <ResourceContext.Provider value={{ resources, addResource, updateResource }}>
      {children}
    </ResourceContext.Provider>
  );
};
